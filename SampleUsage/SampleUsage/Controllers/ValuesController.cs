using Microsoft.AspNetCore.Mvc;
using SampleUsage.Services.Contracts;

namespace SampleUsage.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
		private readonly IFooService _fooService;
		private readonly IBarService _barService;
		private readonly IBazService _bazService;

		public ValuesController(IFooService fooService, IBarService barService, IBazService bazService)
		{
			_fooService = fooService;
			_barService = barService;
			_bazService = bazService;
		}

		[HttpGet, Route("")]
		public JsonResult Index()
		{
			var foo = _fooService.Get();
			var bar = _barService.Get();
			var baz = _bazService.Get();

			return new JsonResult(new { FooMessage = foo, BarMessage = bar, BazMessage = baz });
		}
	}
}