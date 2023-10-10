using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace POI.DiscordDotNet.Controllers;

[ApiController]
[Route("[controller]")]
public class LinkController : ControllerBase
{
	private const string REDIRECT_URL = "http://localhost:5224";


	[HttpGet("scoresaber")]
	public RedirectResult ScoreSaber(string loginToken)
	{
		// Set cookie:
		var cookieOptions = new CookieOptions
		{
			Expires = DateTime.Now.AddMinutes(15), HttpOnly = true,
		};

		Response.Cookies.Append("LoginToken", loginToken, cookieOptions);

		const string baseUrl = "https://steamcommunity.com/openid/login";
		var token = Guid.NewGuid();
		var param = new Dictionary<string, string>()
		{
			{ "openid.ns", "http://specs.openid.net/auth/2.0" },
			{ "openid.mode", "checkid_setup" },
			{ "openid.return_to", $"{REDIRECT_URL}/callback/scoresaber" },
			{ "openid.realm", $"{REDIRECT_URL}" },
			{ "openid.identity", "http://specs.openid.net/auth/2.0/identifier_select" },
			{ "openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" }
		};
		var uriBuilder = new UriBuilder(baseUrl) { Query = string.Join("&", param.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}")) };

		return new RedirectResult(uriBuilder.Uri.ToString());
	}


	[HttpGet("beatleader")]
	public RedirectResult BeatLeader(string loginToken)
	{
		// Set cookie:
		var cookieOptions = new CookieOptions
		{
			Expires = DateTime.Now.AddMinutes(15), HttpOnly = true,
		};

		Response.Cookies.Append("LoginToken", loginToken, cookieOptions);

		const string clientId = "BBSCClientId";
		const string baseUrl = "https://api.beatleader.xyz/oauth2/authorize";
		var param = new Dictionary<string, string>()
		{
			{ "client_id", clientId },
			{ "redirect_uri", $"{REDIRECT_URL}/callback/beatleader" },
			{ "response_type", "code" },
			{ "scope", "openid profile" }
		};
		var uriBuilder = new UriBuilder(baseUrl) { Query = string.Join("&", param.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}")) };
		return new RedirectResult(uriBuilder.Uri.ToString());
	}


	[HttpGet("beatsaver")]
	public RedirectResult BeatSaver(string loginToken)
	{
		// Set cookie:
		var cookieOptions = new CookieOptions
		{
			Expires = DateTime.Now.AddMinutes(15), HttpOnly = true,
		};

		Response.Cookies.Append("LoginToken", loginToken, cookieOptions);

		const string clientId = "BBSCClientId";
		const string baseUrl = "https://api.beatleader.xyz/oauth2/authorize";
		var param = new Dictionary<string, string>()
		{
			{ "client_id", clientId },
			{ "redirect_uri", $"{REDIRECT_URL}/callback/beatsaver" },
			{ "response_type", "code" },
			{ "scope", "openid profile" }
		};
		var uriBuilder = new UriBuilder(baseUrl) { Query = string.Join("&", param.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}")) };
		return new RedirectResult(uriBuilder.Uri.ToString());
	}
}