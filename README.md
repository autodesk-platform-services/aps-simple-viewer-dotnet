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
- Command-line terminal such as [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/overview)
or [bash](https://en.wikipedia.org/wiki/Bash_(Unix_shell)) (should already be available on your system)

> We recommend using [Visual Studio Code](https://code.visualstudio.com) which, among other benefits,
> provides an [integrated terminal](https://code.visualstudio.com/docs/terminal/basics) as well.

### Setup & Run

- Clone this repository: `git clone https://github.com/autodesk-platform-services/aps-simple-viewer-dotnet`
- Go to the project folder: `cd aps-simple-viewer-dotnet`
- Install .NET dependencies: `dotnet restore`
- Open the project folder in a code editor of your choice
- Create an _appsettings.Development.json_ file in the project folder (if it does not exist already),
and populate it with the JSON snippet below, replacing `<client-id>` and `<client-secret>`
with your APS Client ID and Client Secret:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "APS_CLIENT_ID": "<client-id>",
  "APS_CLIENT_SECRET": "<client-secret>"
}
```

- Run the application, either from your code editor, or by running `dotnet run` in terminal
- Open http://localhost:8080

> When using [Visual Studio Code](https://code.visualstudio.com), you can run & debug
> the application by pressing `F5`.

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
