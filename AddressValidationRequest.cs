//-----------------------------------------------------------------------
// <copyright file="AddressValidationRequest.cs" company="Procare Software, LLC">
//     Copyright © 2021-2025 Procare Software, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Procare.AddressValidation.Tester;

using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;

internal record struct AddressValidationRequest(
    string? CompanyName,
    string? Line1,
    string? Line2,
    string? City,
    string? StateCode,
    string? Urbanization,
    string? ZipCodeLeading5,
    string? ZipCodeTrailing4)
: IEquatable<AddressValidationRequest>
{
    public readonly string ToQueryString()
    {
        StringBuilder result = new();

        foreach (PropertyInfo prop in this.GetType().GetProperties())
        {
            string? value = (string?)prop.GetMethod!.Invoke(this, []);
            if (!string.IsNullOrEmpty(value))
            {
                result.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", result.Length == 0 ? "?" : "&", WebUtility.UrlEncode(prop.Name), WebUtility.UrlEncode(value));
            }
        }

        return result.ToString();
    }

    public readonly HttpRequestMessage ToHttpRequest()
    {
        return new HttpRequestMessage(HttpMethod.Get, this.ToQueryString());
    }
}
