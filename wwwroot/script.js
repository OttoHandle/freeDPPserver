// UTF-8 conversion problem with the published script
// Don't copy the script from the publish directory to the root directory of the website -> Copy this script instead

// Global variables
let go_dpp;
let go_dict;
let ga_translations = [];


// Load JSON from path
async function func_Dpp_GetJson(ls_path) {
    // fetch only works on webserver
    let lo_response = await fetch(ls_path, {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        }
    });

    let lo_json = await lo_response.json();

    // For debug
    //console.log(lo_json);

    return lo_json;
}


// Load Dpp
async function func_Dpp_GetDppJson() {
    let ls_url = location;
    go_dpp = await func_Dpp_GetJson(ls_url);
}


// Load Dict
async function func_Dpp_GetDictJson() {
    let lo_crit = go_dpp.elements.find(e => e.elementId == 'c0ProductInformation');

    if (lo_crit) {
        let lo_sectorName = lo_crit.elements.find(e => e.elementId == '_p_d_SectorName');

        if (lo_sectorName) {
            let ls_url = '/properties_sector/' + lo_sectorName.value;

            // Language can be changed with query parameter
            let lo_queryParam = new URLSearchParams(window.location.search);

            if (lo_queryParam.has("language")) {
                ls_url += '?language=' + lo_queryParam.get('language');
            }

            go_dict = await func_Dpp_GetJson(ls_url);
        }
    }
}


// Creates HTML of DPP from JSON Object
function func_Dpp_CreateHtmlFromJson() {
    // Company and DPP Logo
    let lo_logoWrapper = document.createElement('div');
    lo_logoWrapper.classList.add('dppLogosWrapper');
    document.body.appendChild(lo_logoWrapper);

    let lo_companyLogo = document.createElement('div');
    lo_companyLogo.classList.add('dppCompanyLogo');
    lo_companyLogo.ariaLabel = func_Dpp_GetTranslation(1);
    lo_logoWrapper.appendChild(lo_companyLogo);

    let lo_freeDppLogo = document.createElement('div');
    lo_freeDppLogo.classList.add('dppFreeDppLogo');
    lo_freeDppLogo.ariaLabel = func_Dpp_GetTranslation(2);
    lo_logoWrapper.appendChild(lo_freeDppLogo);


    // Wrapper for Pic and Infos
    let lo_topWrapper = document.createElement('div');
    lo_topWrapper.classList.add('dppTopWrapper');


    // Pic
    let lo_topLeft = document.createElement('div');
    lo_topLeft.classList.add('dppTopLeft');

    let lo_picWrapper = document.createElement('div');
    lo_picWrapper.classList.add('dppPicWrapper');
    lo_topLeft.appendChild(lo_picWrapper);

    let lo_pic = document.createElement('img');
    lo_pic.classList.add('dppPic');
    lo_pic.src = '';
    lo_pic.alt = func_Dpp_GetTranslation(3);

    {
        let lo_crit = go_dpp.elements.find(e => e.elementId == 'c0ProductInformation');

        if (lo_crit) {
            let lo_picPath = lo_crit.elements.find(e => e.elementId == '_p_d_ProductImage');

            if (lo_picPath) {
                lo_pic.src = lo_picPath.value;
            }
        }
    }

    lo_picWrapper.appendChild(lo_pic);
    lo_topWrapper.appendChild(lo_topLeft);


    // Infos
    let lo_topRight = document.createElement('div');
    lo_topRight.classList.add('dppTopRight');


    // DPP Name
    let lo_dppNameElement = document.createElement('h2');
    lo_dppNameElement.classList.add('dppName');

    {
        let lo_crit = go_dpp.elements.find(e => e.elementId == 'c0ProductInformation');

        if (lo_crit) {
            let lo_modelName = lo_crit.elements.find(e => e.elementId == '_p_d_ModelName');

            if (lo_modelName) {
                lo_dppNameElement.innerText = lo_modelName.value;
            }
        }
    }

    lo_topRight.appendChild(lo_dppNameElement);


    // Write DPP infos
    func_Dpp_CreateInfo(lo_topRight, 'digitalProductPassportId', go_dpp.digitalProductPassportId);
    func_Dpp_CreateInfo(lo_topRight, 'uniqueProductIdentifier', go_dpp.uniqueProductIdentifier);
    func_Dpp_CreateInfo(lo_topRight, 'granularity', go_dpp.granularity);
    func_Dpp_CreateInfo(lo_topRight, 'dppSchemaVersion', go_dpp.dppSchemaVersion);
    func_Dpp_CreateInfo(lo_topRight, 'dppStatus', go_dpp.dppStatus);
    func_Dpp_CreateInfo(lo_topRight, 'lastUpdated', go_dpp.lastUpdated);
    func_Dpp_CreateInfo(lo_topRight, 'economicOperatorId', go_dpp.economicOperatorId);
    func_Dpp_CreateInfo(lo_topRight, 'facilityId', go_dpp.facilityId);

    let ls_contentSpecificationText = go_dpp.contentSpecificationIds.join(', ');
    func_Dpp_CreateInfo(lo_topRight, 'contentSpecificationIds', ls_contentSpecificationText);

    // Download von JSON einfügen
    {
        let lo_dppInfo = document.createElement('div');
        lo_dppInfo.classList.add('dppInfo');
        lo_topRight.appendChild(lo_dppInfo);

        let lo_dpp_json_link = document.createElement('a');
        lo_dpp_json_link.href = location.pathname + '?contentType=json';
        lo_dpp_json_link.target = '_blank';
        lo_dpp_json_link.classList.add('dppBtn');
        lo_dpp_json_link.innerText = func_Dpp_GetTranslation(4);
        lo_dppInfo.appendChild(lo_dpp_json_link);
    }

    lo_topWrapper.appendChild(lo_topRight);

    document.body.appendChild(lo_topWrapper);


    // Create Elements
    let lo_listWrapper = document.createElement('div');
    lo_listWrapper.classList.add('dppElementsWrapper');

    let lo_list = document.createElement('ul');
    lo_list.classList.add('dppElements');

    go_dpp.elements.forEach(lo_element => {
        func_Dpp_CreateElement(lo_element, lo_list);
    });

    lo_listWrapper.appendChild(lo_list);
    document.body.appendChild(lo_listWrapper);
}


// Creates HTML for a element depending on objectType
// If the element is a dataCollection the function is called for every element in the collection
// Parameter lo_element: element which needs to be inserted as HTML
// Parameter lo_list: HTML element where the newly created element will be inserted
function func_Dpp_CreateElement(lo_element, lo_list) {
    let lo_elementHtml = document.createElement('li');
    lo_elementHtml.classList.add('dppElement');

    let ls_paramName = lo_element.elementId;

    // Get real name from dict
    let lo_property = func_Dpp_GetProperty(lo_element.elementId);
    
    if (lo_property !== undefined) {
        ls_paramName = lo_property.writtenName;
    }

    ls_paramName += ':';

    // Write value or create new lists
    switch (lo_element.objectType) {
        case 'SingleValuedDataElement':
        case 'MultiValuedDataElement':
            // Write element name
            let lo_elementName = document.createElement('span');
            lo_elementName.classList.add('dppElementName');
            lo_elementName.innerText = ls_paramName;
            lo_elementHtml.appendChild(lo_elementName);

            let lo_elementValue = document.createElement('span');
            lo_elementValue.classList.add('dppElementValue');

            if (lo_element.objectType == 'SingleValuedDataElement') {
                // Write only one value

                // Create link if needed
                if (lo_element.value.startsWith('http://') || lo_element.value.startsWith('https://')) {
                    let lo_valueLink = document.createElement('a');
                    lo_valueLink.href = lo_element.value;
                    lo_valueLink.target = '_blank';
                    lo_valueLink.classList.add('dppValueLink');
                    lo_valueLink.innerText = lo_element.value;
                    lo_elementValue.appendChild(lo_valueLink);
                }
                else {
                    lo_elementValue.innerText = lo_element.value;
                }

                if (lo_property !== undefined) {
                    if (lo_property.unit) {
                        lo_elementValue.innerText += ' ' + lo_property.unit.unit;

                        let lo_unitIcon = document.createElement('span');
                        lo_unitIcon.classList.add('dppElementUnitIcon');
                        lo_elementValue.appendChild(lo_unitIcon);
                    }

                }
            }
            else if (lo_element.objectType == 'MultiValuedDataElement') {
                // Write all values
                let la_values = [];

                lo_element.value.forEach(lo_childElement => {
                    let ls_value = lo_childElement.value;

                    if (lo_property !== undefined) {
                        if (lo_property.unit) {
                            ls_value += ' ' + lo_property.unit.unit;
                        }
                    }

                    la_values.push(ls_value);
                });

                lo_elementValue.innerText += la_values.join(', ');
            }

            lo_elementHtml.appendChild(lo_elementValue);
            break;
        case 'DataElementCollection':
            // Add info
            lo_elementHtml.classList.add('isCollection');

            // Clickable Area for opening the collection
            let lo_openElement = document.createElement('div');
            lo_openElement.classList.add('dppOpenElement');
            lo_openElement.title = func_Dpp_GetTranslation(5);
            lo_openElement.addEventListener('click', func_Dpp_SwitchCollectionElement);
            lo_elementHtml.appendChild(lo_openElement);

            // Add icon
            let lo_collIcon = document.createElement('span');
            lo_collIcon.classList.add('dppCollIcon');
            lo_collIcon.ariaLabel = func_Dpp_GetTranslation(6);
            lo_collIcon.style.backgroundImage = `url('/pic/crit/` + lo_element.elementId + `.svg')`;
            lo_openElement.appendChild(lo_collIcon);

            // Write collection name
            let lo_collName = document.createElement('span');
            lo_collName.classList.add('dppElementName');
            lo_collName.innerText = ls_paramName;
            lo_openElement.appendChild(lo_collName);

            // Create element for opening and closing collection
            let lo_switchElement = document.createElement('span');
            lo_switchElement.classList.add('dppCollectionSwitch');
            lo_openElement.appendChild(lo_switchElement);

            // Create new list and call the function for each element in the collection
            let lo_childList = document.createElement('ul');
            lo_childList.classList.add('dppElements');

            // Create Table Header
            let lo_tableHeader = document.createElement('li');
            lo_tableHeader.classList.add('dppElement');
            lo_tableHeader.classList.add('dppElementTableHead');
            lo_childList.appendChild(lo_tableHeader);

            let lo_tableHeaderParam = document.createElement('span');
            lo_tableHeaderParam.classList.add('dppElementName');
            lo_tableHeaderParam.innerText = func_Dpp_GetTranslation(7);
            lo_tableHeader.appendChild(lo_tableHeaderParam);

            let lo_tableHeaderValue = document.createElement('span');
            lo_tableHeaderValue.classList.add('dppElementValue');
            lo_tableHeaderValue.innerText = func_Dpp_GetTranslation(8);
            lo_tableHeader.appendChild(lo_tableHeaderValue);

            // Insert elements in Collection
            if (lo_element.elements) {
                lo_element.elements.forEach(lo_childElement => {
                    func_Dpp_CreateElement(lo_childElement, lo_childList);
                });
            }


            lo_elementHtml.appendChild(lo_childList);
            break;
    }

    lo_list.appendChild(lo_elementHtml);
}


// Create HTML for DPP info
// Parameter ls_infoName: requestet name which is written as HTML
// Parameter lo_dppInfo: JSON Value
function func_Dpp_CreateInfo(lo_parentElement, ls_infoName, ls_dppInfo) {
    let lo_info = document.createElement('div');
    lo_info.classList.add('dppInfo');

    // Name
    let lo_infoName = document.createElement('span');
    lo_infoName.classList.add('dppInfoName');
    lo_infoName.innerText = ls_infoName + ': ';
    lo_info.appendChild(lo_infoName);

    // Value
    let lo_infoValue = document.createElement('span');
    lo_infoValue.classList.add('dppInfoValue');

    // Create link if needed
    if (ls_dppInfo.toString().startsWith('http://') || ls_dppInfo.toString().startsWith('https://')) {
        let lo_infoLink = document.createElement('a');
        lo_infoLink.href = ls_dppInfo;
        lo_infoLink.target = '_blank';
        lo_infoLink.classList.add('dppInfoLink');
        lo_infoLink.innerText = ls_dppInfo;
        lo_infoValue.appendChild(lo_infoLink);
    }
    else {
        lo_infoValue.innerText = ls_dppInfo;
    }

    lo_info.appendChild(lo_infoValue);

    lo_parentElement.appendChild(lo_info);
}


// Event for opening and closing data collections
// Adds or removes class for CSS styling
function func_Dpp_SwitchCollectionElement(lo_event) {
    if (!lo_event.currentTarget.parentElement.classList.contains('open')) {
        lo_event.currentTarget.parentElement.classList.add('open');
    }
    else {
        lo_event.currentTarget.parentElement.classList.remove('open');
    }
}


// Open all collections
function func_Dpp_OpenAllCollections() {
    let la_allCollections = document.querySelectorAll('.dppElement.isCollection');

    la_allCollections.forEach(lo_collection => {
        if (!lo_collection.classList.contains('open')) {
            lo_collection.classList.add('open');
        }
    });
}


// Onload Function
async function func_Dpp_Onload() {
    await func_Dpp_GetDppJson();
    await func_Dpp_GetDictJson();
    func_Dpp_CreateTranslations();
    func_Dpp_CreateHtmlFromJson();
}


// Get property from dict
function func_Dpp_GetProperty(ls_paramName) {
    if (go_dict !== undefined) {
        let lo_property = go_dict.find(p => p.paramName == ls_paramName);
        return lo_property;
    }
    else {
        return undefined;
    }
}


// Create translation
function func_Dpp_CreateTranslation(li_id, ls_language, ls_translation) {
    let lo_translation = {
        Id: li_id,
        Language: ls_language,
        Translation: ls_translation
    };

    ga_translations.push(lo_translation);
}


// Get translation
function func_Dpp_GetTranslation(li_id) {
    let lo_translation = ga_translations.find(t => t.Id == li_id && t.Language == gs_language);

    if (lo_translation === undefined) {
        lo_translation = ga_translations.find(t => t.Id == li_id && t.Language == 'en-GB');
    }

    return lo_translation.Translation;
}


// Create translations
function func_Dpp_CreateTranslations() {
    // Company logo
    func_Dpp_CreateTranslation(1, 'en-GB', 'Company logo');
    func_Dpp_CreateTranslation(1, 'de-DE', 'Firmen Logo');
    func_Dpp_CreateTranslation(1, 'it-IT', 'Logo aziendale');
    func_Dpp_CreateTranslation(1, 'fr-FR', `Logo de l'entreprise`);

    // freedpp logo
    func_Dpp_CreateTranslation(2, 'en-GB', 'freedpp logo');
    func_Dpp_CreateTranslation(2, 'de-DE', 'freedpp Logo');
    func_Dpp_CreateTranslation(2, 'it-IT', 'Logo freedpp');
    func_Dpp_CreateTranslation(2, 'fr-FR', 'Logo freedpp');

    // DPP Image
    func_Dpp_CreateTranslation(3, 'en-GB', 'DPP Image');
    func_Dpp_CreateTranslation(3, 'de-DE', 'DPP Bild');
    func_Dpp_CreateTranslation(3, 'it-IT', 'Foto DPP');
    func_Dpp_CreateTranslation(3, 'fr-FR', 'Image DPP');

    // JSON Download
    func_Dpp_CreateTranslation(4, 'en-GB', 'JSON Download');
    func_Dpp_CreateTranslation(4, 'de-DE', 'JSON-Download');
    func_Dpp_CreateTranslation(4, 'it-IT', 'Scarica JSON');
    func_Dpp_CreateTranslation(4, 'fr-FR', 'Téléchargement JSON');

    // open
    func_Dpp_CreateTranslation(5, 'en-GB', 'open');
    func_Dpp_CreateTranslation(5, 'de-DE', 'öffnen');
    func_Dpp_CreateTranslation(5, 'it-IT', 'aprire');
    func_Dpp_CreateTranslation(5, 'fr-FR', 'ouvrir');

    // Collection icon
    func_Dpp_CreateTranslation(6, 'en-GB', 'Collection icon');
    func_Dpp_CreateTranslation(6, 'de-DE', 'Kollektionssymbol');
    func_Dpp_CreateTranslation(6, 'it-IT', 'Icona della collezione');
    func_Dpp_CreateTranslation(6, 'fr-FR', 'Icône de collection');

    // property
    func_Dpp_CreateTranslation(7, 'en-GB', 'property');
    func_Dpp_CreateTranslation(7, 'de-DE', 'Eigenschaft');
    func_Dpp_CreateTranslation(7, 'it-IT', 'Caratteristica');
    func_Dpp_CreateTranslation(7, 'fr-FR', 'propriété');

    // Value
    func_Dpp_CreateTranslation(8, 'en-GB', 'Value');
    func_Dpp_CreateTranslation(8, 'de-DE', 'Wert');
    func_Dpp_CreateTranslation(8, 'it-IT', 'Valore');
    func_Dpp_CreateTranslation(8, 'fr-FR', 'valeur');

    // ___
    func_Dpp_CreateTranslation(9999, 'en-GB', '___');
    func_Dpp_CreateTranslation(9999, 'de-DE', '___');
    func_Dpp_CreateTranslation(9999, 'it-IT', '___');
    func_Dpp_CreateTranslation(9999, 'fr-FR', '___');
}


// Call function on load
document.addEventListener("DOMContentLoaded", func_Dpp_Onload);