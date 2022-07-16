// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Extensions
{
    public static class CountryExtensions
    {
        /// <summary>
        /// Given a two-letter country code, return the country's name.
        /// </summary>
        /// <param name="code">The two-letter country code, following the ISO 3166-1 standard.</param>
        /// <returns>The country's full name.</returns>
        public static string GetCountryName(string? code)
        {
            switch (code)
            {
                default:
                    return "Unknown";

                case "BD":
                    return "Bangladesh";

                case "BE":
                    return "Belgium";

                case "BF":
                    return "Burkina Faso";

                case "BG":
                    return "Bulgaria";

                case "BA":
                    return "Bosnia and Herzegovina";

                case "BB":
                    return "Barbados";

                case "WF":
                    return "Wallis and Futuna";

                case "BL":
                    return "Saint Barthelemy";

                case "BM":
                    return "Bermuda";

                case "BN":
                    return "Brunei";

                case "BO":
                    return "Bolivia";

                case "BH":
                    return "Bahrain";

                case "BI":
                    return "Burundi";

                case "BJ":
                    return "Benin";

                case "BT":
                    return "Bhutan";

                case "JM":
                    return "Jamaica";

                case "BV":
                    return "Bouvet Island";

                case "BW":
                    return "Botswana";

                case "WS":
                    return "Samoa";

                case "BQ":
                    return "Bonaire, Saint Eustatius and Saba";

                case "BR":
                    return "Brazil";

                case "BS":
                    return "Bahamas";

                case "JE":
                    return "Jersey";

                case "BY":
                    return "Belarus";

                case "BZ":
                    return "Belize";

                case "RU":
                    return "Russia";

                case "RW":
                    return "Rwanda";

                case "RS":
                    return "Serbia";

                case "TL":
                    return "East Timor";

                case "RE":
                    return "Reunion";

                case "TM":
                    return "Turkmenistan";

                case "TJ":
                    return "Tajikistan";

                case "RO":
                    return "Romania";

                case "TK":
                    return "Tokelau";

                case "GW":
                    return "Guinea-Bissau";

                case "GU":
                    return "Guam";

                case "GT":
                    return "Guatemala";

                case "GS":
                    return "South Georgia and the South Sandwich Islands";

                case "GR":
                    return "Greece";

                case "GQ":
                    return "Equatorial Guinea";

                case "GP":
                    return "Guadeloupe";

                case "JP":
                    return "Japan";

                case "GY":
                    return "Guyana";

                case "GG":
                    return "Guernsey";

                case "GF":
                    return "French Guiana";

                case "GE":
                    return "Georgia";

                case "GD":
                    return "Grenada";

                case "GB":
                    return "United Kingdom";

                case "GA":
                    return "Gabon";

                case "SV":
                    return "El Salvador";

                case "GN":
                    return "Guinea";

                case "GM":
                    return "Gambia";

                case "GL":
                    return "Greenland";

                case "GI":
                    return "Gibraltar";

                case "GH":
                    return "Ghana";

                case "OM":
                    return "Oman";

                case "TN":
                    return "Tunisia";

                case "JO":
                    return "Jordan";

                case "HR":
                    return "Croatia";

                case "HT":
                    return "Haiti";

                case "HU":
                    return "Hungary";

                case "HK":
                    return "Hong Kong";

                case "HN":
                    return "Honduras";

                case "HM":
                    return "Heard Island and McDonald Islands";

                case "VE":
                    return "Venezuela";

                case "PR":
                    return "Puerto Rico";

                case "PS":
                    return "Palestinian Territory";

                case "PW":
                    return "Palau";

                case "PT":
                    return "Portugal";

                case "SJ":
                    return "Svalbard and Jan Mayen";

                case "PY":
                    return "Paraguay";

                case "IQ":
                    return "Iraq";

                case "PA":
                    return "Panama";

                case "PF":
                    return "French Polynesia";

                case "PG":
                    return "Papua New Guinea";

                case "PE":
                    return "Peru";

                case "PK":
                    return "Pakistan";

                case "PH":
                    return "Philippines";

                case "PN":
                    return "Pitcairn";

                case "PL":
                    return "Poland";

                case "PM":
                    return "Saint Pierre and Miquelon";

                case "ZM":
                    return "Zambia";

                case "EH":
                    return "Western Sahara";

                case "EE":
                    return "Estonia";

                case "EG":
                    return "Egypt";

                case "ZA":
                    return "South Africa";

                case "EC":
                    return "Ecuador";

                case "IT":
                    return "Italy";

                case "VN":
                    return "Vietnam";

                case "SB":
                    return "Solomon Islands";

                case "ET":
                    return "Ethiopia";

                case "SO":
                    return "Somalia";

                case "ZW":
                    return "Zimbabwe";

                case "SA":
                    return "Saudi Arabia";

                case "ES":
                    return "Spain";

                case "ER":
                    return "Eritrea";

                case "ME":
                    return "Montenegro";

                case "MD":
                    return "Moldova";

                case "MG":
                    return "Madagascar";

                case "MF":
                    return "Saint Martin";

                case "MA":
                    return "Morocco";

                case "MC":
                    return "Monaco";

                case "UZ":
                    return "Uzbekistan";

                case "MM":
                    return "Myanmar";

                case "ML":
                    return "Mali";

                case "MO":
                    return "Macao";

                case "MN":
                    return "Mongolia";

                case "MH":
                    return "Marshall Islands";

                case "MK":
                    return "North Macedonia";

                case "MU":
                    return "Mauritius";

                case "MT":
                    return "Malta";

                case "MW":
                    return "Malawi";

                case "MV":
                    return "Maldives";

                case "MQ":
                    return "Martinique";

                case "MP":
                    return "Northern Mariana Islands";

                case "MS":
                    return "Montserrat";

                case "MR":
                    return "Mauritania";

                case "IM":
                    return "Isle of Man";

                case "UG":
                    return "Uganda";

                case "TZ":
                    return "Tanzania";

                case "MY":
                    return "Malaysia";

                case "MX":
                    return "Mexico";

                case "IL":
                    return "Israel";

                case "FR":
                    return "France";

                case "IO":
                    return "British Indian Ocean Territory";

                case "SH":
                    return "Saint Helena";

                case "FI":
                    return "Finland";

                case "FJ":
                    return "Fiji";

                case "FK":
                    return "Falkland Islands";

                case "FM":
                    return "Micronesia";

                case "FO":
                    return "Faroe Islands";

                case "NI":
                    return "Nicaragua";

                case "NL":
                    return "Netherlands";

                case "NO":
                    return "Norway";

                case "NA":
                    return "Namibia";

                case "VU":
                    return "Vanuatu";

                case "NC":
                    return "New Caledonia";

                case "NE":
                    return "Niger";

                case "NF":
                    return "Norfolk Island";

                case "NG":
                    return "Nigeria";

                case "NZ":
                    return "New Zealand";

                case "NP":
                    return "Nepal";

                case "NR":
                    return "Nauru";

                case "NU":
                    return "Niue";

                case "CK":
                    return "Cook Islands";

                case "XK":
                    return "Kosovo";

                case "CI":
                    return "Ivory Coast";

                case "CH":
                    return "Switzerland";

                case "CO":
                    return "Colombia";

                case "CN":
                    return "China";

                case "CM":
                    return "Cameroon";

                case "CL":
                    return "Chile";

                case "CC":
                    return "Cocos Islands";

                case "CA":
                    return "Canada";

                case "CG":
                    return "Republic of the Congo";

                case "CF":
                    return "Central African Republic";

                case "CD":
                    return "Democratic Republic of the Congo";

                case "CZ":
                    return "Czech Republic";

                case "CY":
                    return "Cyprus";

                case "CX":
                    return "Christmas Island";

                case "CR":
                    return "Costa Rica";

                case "CW":
                    return "Curacao";

                case "CV":
                    return "Cabo Verde";

                case "CU":
                    return "Cuba";

                case "SZ":
                    return "Eswatini";

                case "SY":
                    return "Syria";

                case "SX":
                    return "Sint Maarten";

                case "KG":
                    return "Kyrgyzstan";

                case "KE":
                    return "Kenya";

                case "SS":
                    return "South Sudan";

                case "SR":
                    return "Suriname";

                case "KI":
                    return "Kiribati";

                case "KH":
                    return "Cambodia";

                case "KN":
                    return "Saint Kitts and Nevis";

                case "KM":
                    return "Comoros";

                case "ST":
                    return "Sao Tome and Principe";

                case "SK":
                    return "Slovakia";

                case "KR":
                    return "South Korea";

                case "SI":
                    return "Slovenia";

                case "KP":
                    return "North Korea";

                case "KW":
                    return "Kuwait";

                case "SN":
                    return "Senegal";

                case "SM":
                    return "San Marino";

                case "SL":
                    return "Sierra Leone";

                case "SC":
                    return "Seychelles";

                case "KZ":
                    return "Kazakhstan";

                case "KY":
                    return "Cayman Islands";

                case "SG":
                    return "Singapore";

                case "SE":
                    return "Sweden";

                case "SD":
                    return "Sudan";

                case "DO":
                    return "Dominican Republic";

                case "DM":
                    return "Dominica";

                case "DJ":
                    return "Djibouti";

                case "DK":
                    return "Denmark";

                case "VG":
                    return "British Virgin Islands";

                case "DE":
                    return "Germany";

                case "YE":
                    return "Yemen";

                case "DZ":
                    return "Algeria";

                case "US":
                    return "United States";

                case "UY":
                    return "Uruguay";

                case "YT":
                    return "Mayotte";

                case "UM":
                    return "United States Minor Outlying Islands";

                case "LB":
                    return "Lebanon";

                case "LC":
                    return "Saint Lucia";

                case "LA":
                    return "Laos";

                case "TV":
                    return "Tuvalu";

                case "TW":
                    return "Taiwan";

                case "TT":
                    return "Trinidad and Tobago";

                case "TR":
                    return "Turkey";

                case "LK":
                    return "Sri Lanka";

                case "LI":
                    return "Liechtenstein";

                case "LV":
                    return "Latvia";

                case "TO":
                    return "Tonga";

                case "LT":
                    return "Lithuania";

                case "LU":
                    return "Luxembourg";

                case "LR":
                    return "Liberia";

                case "LS":
                    return "Lesotho";

                case "TH":
                    return "Thailand";

                case "TF":
                    return "French Southern Territories";

                case "TG":
                    return "Togo";

                case "TD":
                    return "Chad";

                case "TC":
                    return "Turks and Caicos Islands";

                case "LY":
                    return "Libya";

                case "VA":
                    return "Vatican";

                case "VC":
                    return "Saint Vincent and the Grenadines";

                case "AE":
                    return "United Arab Emirates";

                case "AD":
                    return "Andorra";

                case "AG":
                    return "Antigua and Barbuda";

                case "AF":
                    return "Afghanistan";

                case "AI":
                    return "Anguilla";

                case "VI":
                    return "U.S. Virgin Islands";

                case "IS":
                    return "Iceland";

                case "IR":
                    return "Iran";

                case "AM":
                    return "Armenia";

                case "AL":
                    return "Albania";

                case "AO":
                    return "Angola";

                case "AQ":
                    return "Antarctica";

                case "AS":
                    return "American Samoa";

                case "AR":
                    return "Argentina";

                case "AU":
                    return "Australia";

                case "AT":
                    return "Austria";

                case "AW":
                    return "Aruba";

                case "IN":
                    return "India";

                case "AX":
                    return "Aland Islands";

                case "AZ":
                    return "Azerbaijan";

                case "IE":
                    return "Ireland";

                case "ID":
                    return "Indonesia";

                case "UA":
                    return "Ukraine";

                case "QA":
                    return "Qatar";

                case "MZ":
                    return "Mozambique";
            }
        }
    }
}
