export const onRequest: PagesFunction = async (context) => {
    const { request } = context;
    const url = new URL(request.url);

    console.log('Pages Function HIT:', request.method, url.pathname, url.search);

    return new Response(
        JSON.stringify({
            message: 'Hello from Cloudflare Pages Functions',
            path: url.pathname,
            query: url.search,
        }),
        {
            status: 200,
            headers: { 'Content-Type': 'application/json' },
        }
    );
};
