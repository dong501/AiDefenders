import json, sys, urllib.request
from pathlib import Path

URL = 'http://127.0.0.1:8080/mcp'
session = None

def rpc(method, params):
    global session
    headers = {'Content-Type': 'application/json', 'Accept': 'application/json, text/event-stream'}
    if session:
        headers['Mcp-Session-Id'] = session
    body = json.dumps({'jsonrpc': '2.0', 'id': 1, 'method': method, 'params': params}).encode()
    with urllib.request.urlopen(urllib.request.Request(URL, data=body, headers=headers), timeout=180) as response:
        session = response.headers.get('Mcp-Session-Id', session)
        raw = response.read().decode()
    if 'data: ' in raw:
        events = [json.loads(line[6:]) for line in raw.splitlines() if line.startswith('data: ')]
        return next(event for event in events if event.get('id') == 1)
    return json.loads(raw)

if __name__ == '__main__':
    rpc('initialize', {'protocolVersion': '2024-11-05', 'capabilities': {}, 'clientInfo': {'name': 'local-editor-review', 'version': '1'}})
    if len(sys.argv) > 1:
        request = {'method': 'tools/call', 'params': {'name': 'execute_code', 'arguments': {'action': 'execute', 'compiler': 'codedom', 'code': Path(sys.argv[1]).read_text(encoding='utf-8-sig')}}}
    else:
        request = json.load(sys.stdin)
    print(json.dumps(rpc(request['method'], request.get('params', {})), ensure_ascii=True))
