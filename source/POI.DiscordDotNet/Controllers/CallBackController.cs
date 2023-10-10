using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using POI.Persistence.Repositories;

namespace POI.DiscordDotNet.Controllers;

[ApiController]
[Route("[controller]")]
public partial class CallBackController : ControllerBase
{
	[GeneratedRegex("([^\\/][0-9]{8,})")]
	private static partial Regex MyRegex();

	private readonly IGlobalUserSettingsRepository _globalUserSettingsRepository;

	public CallBackController(IGlobalUserSettingsRepository globalUserSettingsRepository)
	{
		_globalUserSettingsRepository = globalUserSettingsRepository;
	}


	[HttpGet("scoresaber")]
	public async Task<IActionResult> ScoreSaber([FromQuery(Name = "openid.claimed_id")] string steamId, CancellationToken cts = default)
	{
		var token = HttpContext.Request.Cookies["LoginToken"];
		Response.Cookies.Delete("LoginToken");

		if (!string.IsNullOrWhiteSpace(steamId) && !string.IsNullOrWhiteSpace(token))
		{
			var id = MyRegex().Match(steamId).Groups[1].Value;
			var discordString = GetDiscordIdFromJwtToken(token);
			var discordId = ulong.Parse(discordString!);
			await _globalUserSettingsRepository.CreateOrUpdateScoreSaberLink(discordId, id, cts);
			return RedirectToAction("Index");
		}

		return NotFound();
	}

	[HttpGet("beatleader")]
	public async Task<IActionResult> BeatLeader(string code, CancellationToken cts = default)
	{
		var token = HttpContext.Request.Cookies["LoginToken"];
		Response.Cookies.Delete("LoginToken");

		if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(token))
		{
			var discordId = ulong.Parse(GetDiscordIdFromJwtToken(token)!);
			// var beatLeaderId = await GetBeatLeaderIdFromCode(code);
			var beatLeaderId = "1UL";
			await _globalUserSettingsRepository.CreateOrUpdateBeatLeaderLink(discordId, beatLeaderId, cts);
			return RedirectToAction("Index");
		}

		return NotFound();
	}

	[HttpGet("beatsaver")]
	public async Task<IActionResult> BeatSaver(string code, CancellationToken cts = default)
	{
		var token = HttpContext.Request.Cookies["LoginToken"];
		Response.Cookies.Delete("LoginToken");

		if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(token))
		{
			var discordId = ulong.Parse(GetDiscordIdFromJwtToken(token)!);
			// var beatSaverId = await GetBeatSaverIdFromCode(code);
			var beatSaverId = "1UL";
			await _globalUserSettingsRepository.CreateOrUpdateBeatSaverLink(discordId, beatSaverId, cts);
			return RedirectToAction("Index");
		}

		return NotFound();
	}

	[HttpGet("/")]
	public IActionResult Index()
	{
		return Ok("Account updated!\nYou can close this tab now.");
	}

	private string? GetDiscordIdFromJwtToken(string jwtToken)
	{
		if (string.IsNullOrEmpty(jwtToken))
		{
			return null;
		}

		// Create a JwtSecurityTokenHandler
		var tokenHandler = new JwtSecurityTokenHandler();

		// Read the token
		if (tokenHandler.ReadToken(jwtToken) is not JwtSecurityToken securityToken)
		{
			return null;
		}

		// Find the claim with the Discord ID
		var discordIdClaim = securityToken.Claims.FirstOrDefault(claim => claim.Type == "discordId");

		return discordIdClaim?.Value;
	}
}