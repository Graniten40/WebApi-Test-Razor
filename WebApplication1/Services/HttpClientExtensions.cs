using System.Data;
using System.Net;

using Models;
using Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace Services;
public static class HttpClientExtensions
{
    public static async Task EnsureSuccessStatusMessage(this HttpResponseMessage response)
    {
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex) when (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errMessage = await response.Content.ReadAsStringAsync();

            //rethrow with message recieved
            throw new HttpRequestException($"{response.StatusCode}: {errMessage}", ex, ex.StatusCode);
        }
        catch 
        {
            throw;
        }
    }
}