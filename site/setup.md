# Build and publish documentation

The documentation uses [DocFX](https://dotnet.github.io/docfx/) and GitHub Pages.

## Build locally

Install a supported .NET SDK and run from the repository root:

```bash
dotnet tool install -g docfx
docfx docfx.json
```

The generated static website appears in `_site/`.

## Deployment

The `Publish documentation` GitHub Actions workflow is **manual** until GitHub Pages and the custom domain have been configured. Use **Settings → Pages → Source: GitHub Actions**, then **Actions → Publish documentation → Run workflow**.

Custom domain: `frankenspo.sferix.app`. Register the subdomain as a CNAME to `skvorc-dev.github.io` with the authoritative DNS host. In **Settings → Pages**, enter the same custom domain; wait until DNS and certificate provisioning have completed before enforcing HTTPS.

A checked-in CNAME file does not configure the domain when using GitHub Actions for Pages. The custom domain must be configured in repository settings.
