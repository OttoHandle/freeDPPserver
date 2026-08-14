freeDPPserver
-----
contains the code for the open source DPP server system

- find details here:
https://github.com/OttoHandle/freeDPP/blob/main/FUNCTIONALITY.md

- find licence here:
  https://github.com/OttoHandle/freeDPP/blob/main/LICENSE

- find all modules here:
  https://github.com/OttoHandle


---- first commit 260814 -----

dpp provider according EN 18222 and EN 18223 standards, applicable with link as described in EN 18216

functionality already implemented:
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

functionality missing:
- create DPP
- update DPP and update DPP parts
- delete DPP
- security options according EN 18239 and EN 18246 (not yet published)
- registration in EC registry (not yet possible since datapoint definitions not yet published)
- additional identification schemes according EN 18219
- resolving of more complex identification schema with different routing order (model+serial without batch number inbetween)


Info:
B.3.1.1 Web enabled, structured path identification for products

GTIN (01)				global Trade Item Number			09524000059109
Consumer produc			Used to distinguish variants		2A
variant (22)
Batch number (10)		Batch identifier					ABC123
Serial Number (21)		Unique item-level identifier		12345XYZ
Production date (11)	Format YYMMDD						251121
URL scheme https://example.com
Full Example https://example.com/01/09524000059109/22/2A/10/ABC123/21/12345XYZ?11=251121
								/01/GTIN/22/VARIANT/10/BATCH/21/SERIALNR?11=date

Examples:

https://drill.freedpp.eu/01/5012345101095?contentType=json
https://localhost:7032/v1/dpps/5012345101095?representation=full