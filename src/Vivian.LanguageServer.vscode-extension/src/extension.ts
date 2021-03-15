"use strict";

import { workspace, ExtensionContext } from "vscode";
import 
{
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    TransportKind,
} from "vscode-languageclient/node";

import { Trace, createClientPipeTransport } from "vscode-jsonrpc/node";
// import { createConnection } from "net";
// import * as path from "path";

let client: LanguageClient;

export function activate(context: ExtensionContext)
{
    let serverExe = "dotnet";
    let serverOptions: ServerOptions = 
    {
        run: 
        {
            command: "D:\\VivianLang\\Vivian\\src\\Vivian.LanguageServer\\bin\\Debug\\Vivian.LanguageServer.exe", 
            args: [],
            transport: TransportKind.pipe,
        },
        debug:
        {
            command: serverExe,
            args: ["D:\\VivianLang\\Vivian\\src\\Vivian.LanguageServer\\bin\\Debug\\Vivian.LanguageServer.dll"],
            transport: TransportKind.pipe,
            runtime: "",
        },
    };

    let clientOptions: LanguageClientOptions = 
    {
        documentSelector: [{ pattern: "**/*.viv", }],
        progressOnInitialization: true,
        synchronize: 
        {
            fileEvents: workspace.createFileSystemWatcher("**/*.viv"),
        },
    };

    client = new LanguageClient("Vivian", "Vivian Language Support", serverOptions, clientOptions);
    client.registerProposedFeatures();
    client.trace = Trace.Verbose;
    let disposable = client.start();

    context.subscriptions.push(disposable);
}