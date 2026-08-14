# freeDPPserver

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
-----
> [!IMPORTANT]
> **Not yet for production**


Contains the code for the open source DPP server system

- find details here:
https://github.com/OttoHandle/freeDPP/blob/main/FUNCTIONALITY.md

- find licence here:
  https://github.com/OttoHandle/freeDPP/blob/main/LICENSE

- find all modules here:
  https://github.com/OttoHandle

## Tech Stack

| Component | Technology |
| :--- | :--- |
| **Framework** | .NET 10.0 (ASP.NET Core Web Api) |
| **Data Access** | System.Data.SqlClient (fewer dependencies then Microsoft.Data.SqlClient) |

## Project Structure

```text
freeDPPapi/
|--- freeDPPapi/
| |--- Controllers/
| | |--- ApiV1dppsByProductIdController.cs
| | |--- ApiV1dppsController.cs
| | |--- dppController.cs
| | |--- DppPostController.cs
| | |--- DppValidateController.cs
| | |--- PropertiesSectorController.cs
| | |--- PropertyController.cs
| |--- DppModel/
| | |--- freeDPPapi.csproj.nuget.dgspec.json
| | |--- freeDPPapi.csproj.nuget.g.props
| | |--- freeDPPapi.csproj.nuget.g.targets
| | |--- project.assets.json
| | |--- project.nuget.cache
| |--- obj/
| |--- Properties/
| | |--- launchSettings.json
| |--- wwwroot/
| | |--- pic/
| | | |--- companyLogo.svg
| | | |--- freeDppLogo.svg
| | |--- favicon.ico
| | |--- script.js
| | |--- style.css
|--- AssistDPPdata.cs
|--- AssistDppElementPath.cs
|--- AssistDppPostData.cs
|--- assistInclude.cs
|--- AssistRepoData.cs
|--- assistText.cs
|--- dotnet-tools.json
|--- freeDPPapi.csproj
|--- freedppapi.sln
|--- GetStarted.pdf
|--- LICENSE
|--- Program.cs
|--- PublishedVersion260813.zip
|--- README.md
|--- Startup.cs
```

## Getting Started

See [Get Started](GetStarted.pdf) for instructions on how to set up and run the freeDPPserver.

## Commit History

---- first commit 260814 -----

dpp provider according EN 18222 and EN 18223 standards, applicable with link as described in EN 18216

## Functionality already implemented
- provide dpp as JSON by request with Accept-Header: application/json
- provide dpp HTML framwework by request with Accept-Header: text/html, including JS and CSS files, 
  which can be used to display the dpp in a web browser
- provide JSON also with query parameter "contenttype=json" in the request URL
- provide REST-API for dpp responding on demand
	- compressed version (JSON) on GET /v1/dpp
	- full version (JSON)       on GET /v1/dpp/...?representation=full
- change language responded by query-Parameter ?language=en-EN
- change language responded by HTTP accept-language=en-EN
- provide dictionary based on product sector - note: as flat list of key-value pairs - tree structure needs further implementation
- provide data element definition based on product sector and data element ID as flat list of key value pairs

## Functionality missing
- create DPP
- update DPP and update DPP parts
- delete DPP
- security options according EN 18239 and EN 18246 (not yet published)
- registration in EC registry (not yet possible since datapoint definitions not yet published)
- additional identification schemes according EN 18219
- resolving of more complex identification schema with different routing order (model+serial without batch number inbetween)

## B.3.1.1 Web enabled, structured path identification for products

| Field | Specifier | Example |
| :--- | :--- | :--- |
|global Trade Item Number GTIN-13 or GTIN-14 - any identifier length possible | (01) | 09524000059109 |
| variant | (22) | 2A |
| Batch identifier |  (10) | ABC123 |
| Serial Number (Unique item-level identifier) | (21)	| 12345XYZ |
| Production date (Format YYMMDD) | (11) | 251121 |

URL scheme https://example.com/01/GTIN/22/VARIANT/10/BATCH/21/SERIALNR?11=date

Full Example https://example.com/01/09524000059109/22/2A/10/ABC123/21/12345XYZ?11=251121

Note: DPP Identifier is definied in 18219 as full path including all identifiers, but the DPP server can also respond to partial paths, e.g. only GTIN-13 or GTIN-14, or GTIN-13 with variant and batch number.								

## Examples

https://drill.freedpp.eu/01/5012345101095?contentType=json

https://localhost:7032/v1/dpps/5012345101095?representation=full