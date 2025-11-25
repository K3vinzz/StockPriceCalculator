interface Env {
    GCP_SA_KEY: string;
    CLOUD_RUN_BASE_URL: string;
}


export const onRequest: PagesFunction<Env> = async (context) => {
    const { request, env } = context;

    const saKeyJson = env.GCP_SA_KEY as string;
    const cloudRunBase = env.CLOUD_RUN_BASE_URL as string;

    if (!saKeyJson || !cloudRunBase) {
        return new Response("Missing GCP_SA_KEY or CLOUD_RUN_BASE_URL", { status: 500 });
    }

    const sa = JSON.parse(saKeyJson);

    // ----- 1. 準備 JWT -----
    const header = { alg: "RS256", typ: "JWT" };
    const now = Math.floor(Date.now() / 1000);
    const claim = {
        iss: sa.client_email,
        scope: "https://www.googleapis.com/auth/cloud-platform",
        aud: "https://oauth2.googleapis.com/token",
        iat: now,
        exp: now + 3600,
    };

    const base64UrlEncode = (obj: unknown) =>
        btoa(JSON.stringify(obj))
            .replace(/\+/g, "-")
            .replace(/\//g, "_")
            .replace(/=+$/, "");

    const unsignedJwt = `${base64UrlEncode(header)}.${base64UrlEncode(claim)}`;

    const pem = sa.private_key as string;
    const pemBody = pem
        .replace("-----BEGIN PRIVATE KEY-----", "")
        .replace("-----END PRIVATE KEY-----", "")
        .replace(/\s+/g, "");

    const keyData = Uint8Array.from(atob(pemBody), (c) => c.charCodeAt(0));

    const cryptoKey = await crypto.subtle.importKey(
        "pkcs8",
        keyData.buffer,
        { name: "RSASSA-PKCS1-v1_5", hash: "SHA-256" },
        false,
        ["sign"]
    );

    const signature = await crypto.subtle.sign(
        "RSASSA-PKCS1-v1_5",
        cryptoKey,
        new TextEncoder().encode(unsignedJwt)
    );

    const signatureBase64Url = btoa(
        String.fromCharCode(...new Uint8Array(signature))
    )
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/, "");

    const signedJwt = `${unsignedJwt}.${signatureBase64Url}`;

    // ----- 2. 用 JWT 換 Access Token -----
    const tokenRes = await fetch("https://oauth2.googleapis.com/token", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: new URLSearchParams({
            grant_type: "urn:ietf:params:oauth:grant-type:jwt-bearer",
            assertion: signedJwt,
        }),
    });

    if (!tokenRes.ok) {
        const text = await tokenRes.text();
        return new Response("Failed to get token: " + text, { status: 500 });
    }

    const tokenJson = await tokenRes.json<any>();
    const accessToken = tokenJson.access_token as string;

    // ----- 3. Proxy request 到 Cloud Run -----
    const url = new URL(request.url);

    // 前端打的是 /api/stock/CalculateSettlement
    // Cloud Run 也是 /api/stock/CalculateSettlement
    const targetUrl = new URL(cloudRunBase);
    targetUrl.pathname = url.pathname;
    targetUrl.search = url.search;

    const headers = new Headers(request.headers);
    ["host", "cf-connecting-ip", "x-forwarded-for", "x-forwarded-host"].forEach(h => headers.delete(h));

    headers.set("Authorization", `Bearer ${accessToken}`);

    const init: RequestInit = {
        method: request.method,
        headers,
    };

    if (request.method !== "GET" && request.method !== "HEAD") {
        init.body = request.body;
    }

    const apiRes = await fetch(targetUrl.toString(), init);

    return new Response(apiRes.body, {
        status: apiRes.status,
        headers: apiRes.headers,
    });
};
