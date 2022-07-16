// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Users
{
    public class Country : IEquatable<Country>
    {
        /// <summary>
        /// The country's full name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// The two-letter country code, following the ISO 3166-1 standard.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; private set; }

        [JsonConstructor]
        private Country(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public bool Equals(Country? other) => Code == other?.Code;

        /// <summary>
        /// Given a two-letter country code, return the respective <see cref="Country"/>.
        /// </summary>
        /// <param name="code">The two-letter country code, following the ISO 3166-1 standard.</param>
        /// <returns>The respective <see cref="Country"/>, or null if the code is unknown.</returns>
        public static Country? FromCode(string code)
        {
            switch (code)
            {
                case "BD":
                    return new Country("Bangladesh", code);

                case "BE":
                    return new Country("Belgium", code);

                case "BF":
                    return new Country("Burkina Faso", code);

                case "BG":
                    return new Country("Bulgaria", code);

                case "BA":
                    return new Country("Bosnia and Herzegovina", code);

                case "BB":
                    return new Country("Barbados", code);

                case "WF":
                    return new Country("Wallis and Futuna", code);

                case "BL":
                    return new Country("Saint Barthelemy", code);

                case "BM":
                    return new Country("Bermuda", code);

                case "BN":
                    return new Country("Brunei", code);

                case "BO":
                    return new Country("Bolivia", code);

                case "BH":
                    return new Country("Bahrain", code);

                case "BI":
                    return new Country("Burundi", code);

                case "BJ":
                    return new Country("Benin", code);

                case "BT":
                    return new Country("Bhutan", code);

                case "JM":
                    return new Country("Jamaica", code);

                case "BV":
                    return new Country("Bouvet Island", code);

                case "BW":
                    return new Country("Botswana", code);

                case "WS":
                    return new Country("Samoa", code);

                case "BQ":
                    return new Country("Bonaire, Saint Eustatius and Saba", code);

                case "BR":
                    return new Country("Brazil", code);

                case "BS":
                    return new Country("Bahamas", code);

                case "JE":
                    return new Country("Jersey", code);

                case "BY":
                    return new Country("Belarus", code);

                case "BZ":
                    return new Country("Belize", code);

                case "RU":
                    return new Country("Russia", code);

                case "RW":
                    return new Country("Rwanda", code);

                case "RS":
                    return new Country("Serbia", code);

                case "TL":
                    return new Country("East Timor", code);

                case "RE":
                    return new Country("Reunion", code);

                case "TM":
                    return new Country("Turkmenistan", code);

                case "TJ":
                    return new Country("Tajikistan", code);

                case "RO":
                    return new Country("Romania", code);

                case "TK":
                    return new Country("Tokelau", code);

                case "GW":
                    return new Country("Guinea-Bissau", code);

                case "GU":
                    return new Country("Guam", code);

                case "GT":
                    return new Country("Guatemala", code);

                case "GS":
                    return new Country("South Georgia and the South Sandwich Islands", code);

                case "GR":
                    return new Country("Greece", code);

                case "GQ":
                    return new Country("Equatorial Guinea", code);

                case "GP":
                    return new Country("Guadeloupe", code);

                case "JP":
                    return new Country("Japan", code);

                case "GY":
                    return new Country("Guyana", code);

                case "GG":
                    return new Country("Guernsey", code);

                case "GF":
                    return new Country("French Guiana", code);

                case "GE":
                    return new Country("Georgia", code);

                case "GD":
                    return new Country("Grenada", code);

                case "GB":
                    return new Country("United Kingdom", code);

                case "GA":
                    return new Country("Gabon", code);

                case "SV":
                    return new Country("El Salvador", code);

                case "GN":
                    return new Country("Guinea", code);

                case "GM":
                    return new Country("Gambia", code);

                case "GL":
                    return new Country("Greenland", code);

                case "GI":
                    return new Country("Gibraltar", code);

                case "GH":
                    return new Country("Ghana", code);

                case "OM":
                    return new Country("Oman", code);

                case "TN":
                    return new Country("Tunisia", code);

                case "JO":
                    return new Country("Jordan", code);

                case "HR":
                    return new Country("Croatia", code);

                case "HT":
                    return new Country("Haiti", code);

                case "HU":
                    return new Country("Hungary", code);

                case "HK":
                    return new Country("Hong Kong", code);

                case "HN":
                    return new Country("Honduras", code);

                case "HM":
                    return new Country("Heard Island and McDonald Islands", code);

                case "VE":
                    return new Country("Venezuela", code);

                case "PR":
                    return new Country("Puerto Rico", code);

                case "PS":
                    return new Country("Palestinian Territory", code);

                case "PW":
                    return new Country("Palau", code);

                case "PT":
                    return new Country("Portugal", code);

                case "SJ":
                    return new Country("Svalbard and Jan Mayen", code);

                case "PY":
                    return new Country("Paraguay", code);

                case "IQ":
                    return new Country("Iraq", code);

                case "PA":
                    return new Country("Panama", code);

                case "PF":
                    return new Country("French Polynesia", code);

                case "PG":
                    return new Country("Papua New Guinea", code);

                case "PE":
                    return new Country("Peru", code);

                case "PK":
                    return new Country("Pakistan", code);

                case "PH":
                    return new Country("Philippines", code);

                case "PN":
                    return new Country("Pitcairn", code);

                case "PL":
                    return new Country("Poland", code);

                case "PM":
                    return new Country("Saint Pierre and Miquelon", code);

                case "ZM":
                    return new Country("Zambia", code);

                case "EH":
                    return new Country("Western Sahara", code);

                case "EE":
                    return new Country("Estonia", code);

                case "EG":
                    return new Country("Egypt", code);

                case "ZA":
                    return new Country("South Africa", code);

                case "EC":
                    return new Country("Ecuador", code);

                case "IT":
                    return new Country("Italy", code);

                case "VN":
                    return new Country("Vietnam", code);

                case "SB":
                    return new Country("Solomon Islands", code);

                case "ET":
                    return new Country("Ethiopia", code);

                case "SO":
                    return new Country("Somalia", code);

                case "ZW":
                    return new Country("Zimbabwe", code);

                case "SA":
                    return new Country("Saudi Arabia", code);

                case "ES":
                    return new Country("Spain", code);

                case "ER":
                    return new Country("Eritrea", code);

                case "ME":
                    return new Country("Montenegro", code);

                case "MD":
                    return new Country("Moldova", code);

                case "MG":
                    return new Country("Madagascar", code);

                case "MF":
                    return new Country("Saint Martin", code);

                case "MA":
                    return new Country("Morocco", code);

                case "MC":
                    return new Country("Monaco", code);

                case "UZ":
                    return new Country("Uzbekistan", code);

                case "MM":
                    return new Country("Myanmar", code);

                case "ML":
                    return new Country("Mali", code);

                case "MO":
                    return new Country("Macao", code);

                case "MN":
                    return new Country("Mongolia", code);

                case "MH":
                    return new Country("Marshall Islands", code);

                case "MK":
                    return new Country("North Macedonia", code);

                case "MU":
                    return new Country("Mauritius", code);

                case "MT":
                    return new Country("Malta", code);

                case "MW":
                    return new Country("Malawi", code);

                case "MV":
                    return new Country("Maldives", code);

                case "MQ":
                    return new Country("Martinique", code);

                case "MP":
                    return new Country("Northern Mariana Islands", code);

                case "MS":
                    return new Country("Montserrat", code);

                case "MR":
                    return new Country("Mauritania", code);

                case "IM":
                    return new Country("Isle of Man", code);

                case "UG":
                    return new Country("Uganda", code);

                case "TZ":
                    return new Country("Tanzania", code);

                case "MY":
                    return new Country("Malaysia", code);

                case "MX":
                    return new Country("Mexico", code);

                case "IL":
                    return new Country("Israel", code);

                case "FR":
                    return new Country("France", code);

                case "IO":
                    return new Country("British Indian Ocean Territory", code);

                case "SH":
                    return new Country("Saint Helena", code);

                case "FI":
                    return new Country("Finland", code);

                case "FJ":
                    return new Country("Fiji", code);

                case "FK":
                    return new Country("Falkland Islands", code);

                case "FM":
                    return new Country("Micronesia", code);

                case "FO":
                    return new Country("Faroe Islands", code);

                case "NI":
                    return new Country("Nicaragua", code);

                case "NL":
                    return new Country("Netherlands", code);

                case "NO":
                    return new Country("Norway", code);

                case "NA":
                    return new Country("Namibia", code);

                case "VU":
                    return new Country("Vanuatu", code);

                case "NC":
                    return new Country("New Caledonia", code);

                case "NE":
                    return new Country("Niger", code);

                case "NF":
                    return new Country("Norfolk Island", code);

                case "NG":
                    return new Country("Nigeria", code);

                case "NZ":
                    return new Country("New Zealand", code);

                case "NP":
                    return new Country("Nepal", code);

                case "NR":
                    return new Country("Nauru", code);

                case "NU":
                    return new Country("Niue", code);

                case "CK":
                    return new Country("Cook Islands", code);

                case "XK":
                    return new Country("Kosovo", code);

                case "CI":
                    return new Country("Ivory Coast", code);

                case "CH":
                    return new Country("Switzerland", code);

                case "CO":
                    return new Country("Colombia", code);

                case "CN":
                    return new Country("China", code);

                case "CM":
                    return new Country("Cameroon", code);

                case "CL":
                    return new Country("Chile", code);

                case "CC":
                    return new Country("Cocos Islands", code);

                case "CA":
                    return new Country("Canada", code);

                case "CG":
                    return new Country("Republic of the Congo", code);

                case "CF":
                    return new Country("Central African Republic", code);

                case "CD":
                    return new Country("Democratic Republic of the Congo", code);

                case "CZ":
                    return new Country("Czech Republic", code);

                case "CY":
                    return new Country("Cyprus", code);

                case "CX":
                    return new Country("Christmas Island", code);

                case "CR":
                    return new Country("Costa Rica", code);

                case "CW":
                    return new Country("Curacao", code);

                case "CV":
                    return new Country("Cabo Verde", code);

                case "CU":
                    return new Country("Cuba", code);

                case "SZ":
                    return new Country("Eswatini", code);

                case "SY":
                    return new Country("Syria", code);

                case "SX":
                    return new Country("Sint Maarten", code);

                case "KG":
                    return new Country("Kyrgyzstan", code);

                case "KE":
                    return new Country("Kenya", code);

                case "SS":
                    return new Country("South Sudan", code);

                case "SR":
                    return new Country("Suriname", code);

                case "KI":
                    return new Country("Kiribati", code);

                case "KH":
                    return new Country("Cambodia", code);

                case "KN":
                    return new Country("Saint Kitts and Nevis", code);

                case "KM":
                    return new Country("Comoros", code);

                case "ST":
                    return new Country("Sao Tome and Principe", code);

                case "SK":
                    return new Country("Slovakia", code);

                case "KR":
                    return new Country("South Korea", code);

                case "SI":
                    return new Country("Slovenia", code);

                case "KP":
                    return new Country("North Korea", code);

                case "KW":
                    return new Country("Kuwait", code);

                case "SN":
                    return new Country("Senegal", code);

                case "SM":
                    return new Country("San Marino", code);

                case "SL":
                    return new Country("Sierra Leone", code);

                case "SC":
                    return new Country("Seychelles", code);

                case "KZ":
                    return new Country("Kazakhstan", code);

                case "KY":
                    return new Country("Cayman Islands", code);

                case "SG":
                    return new Country("Singapore", code);

                case "SE":
                    return new Country("Sweden", code);

                case "SD":
                    return new Country("Sudan", code);

                case "DO":
                    return new Country("Dominican Republic", code);

                case "DM":
                    return new Country("Dominica", code);

                case "DJ":
                    return new Country("Djibouti", code);

                case "DK":
                    return new Country("Denmark", code);

                case "VG":
                    return new Country("British Virgin Islands", code);

                case "DE":
                    return new Country("Germany", code);

                case "YE":
                    return new Country("Yemen", code);

                case "DZ":
                    return new Country("Algeria", code);

                case "US":
                    return new Country("United States", code);

                case "UY":
                    return new Country("Uruguay", code);

                case "YT":
                    return new Country("Mayotte", code);

                case "UM":
                    return new Country("United States Minor Outlying Islands", code);

                case "LB":
                    return new Country("Lebanon", code);

                case "LC":
                    return new Country("Saint Lucia", code);

                case "LA":
                    return new Country("Laos", code);

                case "TV":
                    return new Country("Tuvalu", code);

                case "TW":
                    return new Country("Taiwan", code);

                case "TT":
                    return new Country("Trinidad and Tobago", code);

                case "TR":
                    return new Country("Turkey", code);

                case "LK":
                    return new Country("Sri Lanka", code);

                case "LI":
                    return new Country("Liechtenstein", code);

                case "LV":
                    return new Country("Latvia", code);

                case "TO":
                    return new Country("Tonga", code);

                case "LT":
                    return new Country("Lithuania", code);

                case "LU":
                    return new Country("Luxembourg", code);

                case "LR":
                    return new Country("Liberia", code);

                case "LS":
                    return new Country("Lesotho", code);

                case "TH":
                    return new Country("Thailand", code);

                case "TF":
                    return new Country("French Southern Territories", code);

                case "TG":
                    return new Country("Togo", code);

                case "TD":
                    return new Country("Chad", code);

                case "TC":
                    return new Country("Turks and Caicos Islands", code);

                case "LY":
                    return new Country("Libya", code);

                case "VA":
                    return new Country("Vatican", code);

                case "VC":
                    return new Country("Saint Vincent and the Grenadines", code);

                case "AE":
                    return new Country("United Arab Emirates", code);

                case "AD":
                    return new Country("Andorra", code);

                case "AG":
                    return new Country("Antigua and Barbuda", code);

                case "AF":
                    return new Country("Afghanistan", code);

                case "AI":
                    return new Country("Anguilla", code);

                case "VI":
                    return new Country("U.S. Virgin Islands", code);

                case "IS":
                    return new Country("Iceland", code);

                case "IR":
                    return new Country("Iran", code);

                case "AM":
                    return new Country("Armenia", code);

                case "AL":
                    return new Country("Albania", code);

                case "AO":
                    return new Country("Angola", code);

                case "AQ":
                    return new Country("Antarctica", code);

                case "AS":
                    return new Country("American Samoa", code);

                case "AR":
                    return new Country("Argentina", code);

                case "AU":
                    return new Country("Australia", code);

                case "AT":
                    return new Country("Austria", code);

                case "AW":
                    return new Country("Aruba", code);

                case "IN":
                    return new Country("India", code);

                case "AX":
                    return new Country("Aland Islands", code);

                case "AZ":
                    return new Country("Azerbaijan", code);

                case "IE":
                    return new Country("Ireland", code);

                case "ID":
                    return new Country("Indonesia", code);

                case "UA":
                    return new Country("Ukraine", code);

                case "QA":
                    return new Country("Qatar", code);

                case "MZ":
                    return new Country("Mozambique", code);
            }

            return null;
        }
    }
}
