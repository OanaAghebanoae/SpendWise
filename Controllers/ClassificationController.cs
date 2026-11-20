using Microsoft.AspNetCore.Mvc;
using SpendWise.DTOs;
using SpendWise.Services;

namespace SpendWise.Controllers;

[ApiController]
[Route("api/classification")]
public sealed class ClassificationController(ITransactionClassifier classifier) : ControllerBase
{
    [HttpPost("proposals")]
    public async Task<ActionResult<IReadOnlyList<ClassificationProposalDto>>> Propose(CancellationToken cancellationToken) => Ok(await classifier.ProposeAsync(cancellationToken));

    [HttpPost("apply")]
    public async Task<ActionResult<ClassificationApplyResultDto>> Apply(CancellationToken cancellationToken) => Ok(await classifier.ApplyAsync(cancellationToken));
}
