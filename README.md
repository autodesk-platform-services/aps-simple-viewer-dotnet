# Simple Viewer (.NET)

![platforms](https://img.shields.io/badge/platform-windows%20%7C%20osx%20%7C%20linux-lightgray.svg)
[![.net](https://img.shields.io/badge/net-6.0-blue.svg)](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
[![license](https://img.shields.io/:license-mit-green.svg)](https://opensource.org/licenses/MIT)

[Autodesk Platform Services](https://forge.autodesk.com) application built by following
the [Simple Viewer](https://tutorials.autodesk.io/tutorials/simple-viewer/) tutorial
from https://tutorials.autodesk.io.

![thumbnail](thumbnail.png)

## Development

### Prerequisites

- [APS credentials](https://forge.autodesk.com/en/docs/oauth/v2/tutorials/create-app)
- [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- Terminal (for example, [Windows Command Prompt](https://en.wikipedia.org/wiki/Cmd.exe)
or [macOS Terminal](https://support.apple.com/guide/terminal/welcome/mac))

### Setup & Run

- Clone this repository
- Install dependencies: `dotnet restore`
- Setup environment variables:
  - `APS_CLIENT_ID` - your APS application client ID
  - `APS_CLIENT_SECRET` - your APS application client secret
  - `APS_BUCKET` (optional) - name of APS bucket to store your designs in
- Run the server: `dotnet run`

> When using [Visual Studio Code](https://code.visualstudio.com),
you can specify the env. variables listed above in a _.env_ file in this
folder, and run & debug the application directly from the editor.

## Troubleshooting

### Invalid active developer path

If you're getting `invalid active developer path` errors, please follow the steps
explained in https://apple.stackexchange.com/questions/254380/why-am-i-getting-an-invalid-active-developer-path-when-attempting-to-use-git-a.

### Dev-certs errors

If you're seeing errors related to `dev-certs`, try cleaning any existing certificates
as explained in https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide,
and on macOS and Windows systems, do not forget to use the `--trust` switch.

If you have any other question, please contact us via https://forge.autodesk.com/en/support/get-help.

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT).
Please see the [LICENSE](LICENSE) file for more details.
