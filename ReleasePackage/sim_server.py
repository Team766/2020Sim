#!/usr/bin/env python3

import asyncio
import logging
import os.path
import pprint

import aiohttp
from aiohttp import web

WS_SERVER = "http://localhost:7778"
ROBOT_SERVER = "http://localhost:5800"

FILES_DIR = os.path.dirname(__file__)

logger = logging.getLogger(__name__)

async def index_handler(request):
    return web.FileResponse(os.path.join(FILES_DIR, "html", "index.html"))

async def websocket_handler(req):
    logger.info(f"##### WS_CONNECT")
    ws_server = web.WebSocketResponse()
    await ws_server.prepare(req)
    logger.info(f"##### WS_SERVER {pprint.pformat(ws_server)}")

    async with (
        aiohttp.ClientSession(cookies=req.cookies) as client_session,
        client_session.ws_connect(WS_SERVER + req.path_qs) as ws_client,
    ):
        logger.info(f"##### WS_CLIENT {pprint.pformat(ws_client)}")

        async def wsforward(ws_from, ws_to):
            async for msg in ws_from:
                # logger.info(f">>> msg: {pprint.pformat(msg)}")
                mt = msg.type
                md = msg.data
                if mt == aiohttp.WSMsgType.TEXT:
                    await ws_to.send_str(md)
                elif mt == aiohttp.WSMsgType.BINARY:
                    await ws_to.send_bytes(md)
                elif mt == aiohttp.WSMsgType.PING:
                    await ws_to.ping()
                elif mt == aiohttp.WSMsgType.PONG:
                    await ws_to.pong()
                elif ws_to.closed:
                    await ws_to.close(code=ws_to.close_code, message=msg.extra)
                else:
                    raise ValueError(f"unexpected message type: {pprint.pformat(msg)}")

        await asyncio.wait(
            [
                asyncio.create_task(wsforward(ws_server, ws_client)),
                asyncio.create_task(wsforward(ws_client, ws_server)),
            ],
            return_when=asyncio.FIRST_COMPLETED,
        )

    return ws_server

async def robot_handler(request):
    CHUNK_SIZE = 32768
    async with aiohttp.client.request(
        request.method, f"{ROBOT_SERVER}" + request.path_qs,
        headers=request.headers,
        chunked=CHUNK_SIZE,
    ) as r:
        response = aiohttp.web.StreamResponse(status=r.status, headers=r.headers)
        await response.prepare(request)
        content = r.content
        while True:
            chunk = await content.read(CHUNK_SIZE)
            if not chunk:
                break
            await response.write(chunk)

    await response.write_eof()
    return response

def main():
    app = web.Application()
    app.add_routes([
        web.get("/", index_handler),
        web.static("/sim", os.path.join(FILES_DIR, "html/sim")),
        web.get("/ws", websocket_handler),
        web.get(r"/{tail:.+}", robot_handler),
    ])
    web.run_app(app, port=8000)

if __name__ == "__main__":
    logging.basicConfig(level=logging.INFO)
    logging.getLogger('aiohttp.access').setLevel(logging.WARNING)
    main()