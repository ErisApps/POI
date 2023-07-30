using POI.DiscordDotNet.Commands.SlashCommands.ScoreSaber;

namespace POI.DiscordDotNet.Models;

public record LayerResponse
{
	private readonly List<string> _errorMessages = new();

	public bool Success => _errorMessages.Count == 0;

	public IReadOnlyList<string> ErrorMessages => _errorMessages;

	public virtual LayerResponse AddError(string errorMessage)
	{
		_errorMessages.Add(errorMessage);
		return this;
	}

	public static LayerResponse CreateSuccess() => new();
}

public record LayerResponse<TResult> : LayerResponse
{
	public TResult? Result { get; init; }

	public override LayerResponse<TResult> AddError(string errorMessage)
	{
		base.AddError(errorMessage);
		return this;
	}

	public static LayerResponse RebuildWithoutResult(LayerResponse<TResult> response)
	{
		var newResponse = new LayerResponse();
		foreach (var error in response.ErrorMessages)
		{
			newResponse.AddError(error);
		}

		return newResponse;
	}

	public static LayerResponse<TResult> CreateSuccess(TResult? result = default) => new() { Result = result };
}