using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using POI.Persistence.Repositories;

namespace POI.Web.API.Controllers;

[ApiController]
[Route("[controller]")]
public partial class CallBackController: ControllerBase
{
	[GeneratedRegex("([^\\/][0-9]{8,})")]
	private static partial Regex MyRegex();
	private readonly IGlobalUserSettingsRepository _globalUserSettingsRepository;
	private readonly ILinkRequestTokensRepository _tokenRepository;

	public CallBackController(IGlobalUserSettingsRepository globalUserSettingsRepository, ILinkRequestTokensRepository tokenRepository)
	{
		_globalUserSettingsRepository = globalUserSettingsRepository;
		_tokenRepository = tokenRepository;
	}


	[HttpGet("scoresaber")]
	public async Task<string> ScoreSaber([FromQuery(Name = "openid.claimed_id")] string steamId, CancellationToken cts = default)
	{
		var token = HttpContext.Request.Cookies["LoginToken"];
		Response.Cookies.Delete("LoginToken");

		if (!string.IsNullOrWhiteSpace(steamId) && !string.IsNullOrWhiteSpace(token))
		{
			var id = MyRegex().Match(steamId).Groups[1].Value;
			var discordId = await _tokenRepository.GetDiscordIdByToken(token);
			await _globalUserSettingsRepository.CreateOrUpdateScoreSaberLink(discordId, id, cts);
			return "token:" + token + "Steam!!! " + id;
		}

		return "Not found!";
	}

	[HttpGet("beatleader")]
	public async Task<string> BeatLeader(string code, CancellationToken cts = default)
	{
		var token = HttpContext.Request.Cookies["LoginToken"];
		Response.Cookies.Delete("LoginToken");

		if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(token))
		{
			var discordId = await _tokenRepository.GetDiscordIdByToken(token);
			// var beatLeaderId = await GetBeatLeaderIdFromCode(code);
			var beatLeaderId = "1UL";
			await _globalUserSettingsRepository.CreateOrUpdateBeatLeaderLink(discordId, beatLeaderId, cts);
			return "token:" + token + "BeatLeader!!! " + code;
		}

		return "Not found!";
	}

	[HttpGet("beatsaver")]
	public async Task<string> BeatSaver(string code, CancellationToken cts = default)
	{
		var token = HttpContext.Request.Cookies["LoginToken"];
		Response.Cookies.Delete("LoginToken");

		if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(token))
		{
			var discordId = await _tokenRepository.GetDiscordIdByToken(token);
			// var beatSaverId = await GetBeatSaverIdFromCode(code);
			var beatSaverId = "1UL";
			await _globalUserSettingsRepository.CreateOrUpdateBeatSaverLink(discordId, beatSaverId, cts);
			return "token:" + token + "BeatSaver!!! " + code;
		}

		return "Not found!";
	}
}