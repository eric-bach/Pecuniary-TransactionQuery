using System;
using System.Threading.Tasks;
using EricBach.LambdaLogger;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pecuniary.Transaction.Data.Models;
using Pecuniary.Transaction.Query.Queries;

namespace Pecuniary.Transaction.Query.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransactionController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        // GET api/transaction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionReadModel>> GetAsync(Guid id)
        {
            Logger.Log($"Received {nameof(GetTransactionQuery)} for {id}");

            return await _mediator.Send(new GetTransactionQuery { Id = id});
        }
    }
}
