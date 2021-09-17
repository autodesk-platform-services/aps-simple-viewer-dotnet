# forge-simple-viewer-dotnet

> This is currently a work-in-progress.

Sample [Autodesk Forge](https://forge.autodesk.com) application attempting to provide a cleaner,
easier-to-read implementation of the "View Models" application from https://learnforge.autodesk.io.

## Troubleshooting

### Invalid active developer path

If you're getting `invalid active developer path` errors, please follow the steps
explained in https://apple.stackexchange.com/questions/254380/why-am-i-getting-an-invalid-active-developer-path-when-attempting-to-use-git-a.

### Dev-certs errors

If you're seeing errors related to `dev-certs`, try cleaning any existing certificates
as explained in https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide,
and on macOS and Windows systems, do not forget to use the `--trust` switch.
