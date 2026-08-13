using Application.DTOs;
using Application.Features.Stats.Queries;
using Application.Features.Traps.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly IMediator _mediator;

        // ✅ Constructor injection – required
        public StatsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("rodent-distribution")]
        public async Task<IActionResult> GetRodentDistribution([FromQuery] string? groupNumber)
        {
            var query = new GetRodentTypeDistributionQuery(groupNumber);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpGet("rodent-activity")]
        public async Task<IActionResult> GetRodentActivity(
    [FromQuery] string? groupNumber,
    [FromQuery] string? status,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetRodentActivityQuery(groupNumber, status, fromDate, toDate));
            return Ok(result);
        }
        [HttpGet("rodent-activity-details")]
        public async Task<IActionResult> GetRodentActivityDetails(
     [FromQuery] string? groupNumber,
     [FromQuery] string? status,
     [FromQuery] DateTime? fromDate,
     [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetRodentActivityDetailsQuery(groupNumber, status, fromDate, toDate));
            return Ok(result);
        }
        [HttpGet("latest-alert-time")]
        public async Task<IActionResult> GetLatestAlertTime()
        {
            var result = await _mediator.Send(new GetLatestAlertTimeQuery());
            return Ok(result);
        }

        // ✅ مؤشر شدة الاصابة (Infestation Severity Indicator)
        //[HttpGet("intensity-indicator")]
        //public async Task<IActionResult> GetIntensityIndicator()
        //{
        //    var result = await _mediator.Send(new GetIntensityIndicatorQuery());
        //    return Ok(new { Percentage = result });
        //}

        //// ✅ خريطة مؤشر الشدة المشتتة (Heatmap Scatter Data)
        //[HttpGet("heatmap-scatter-data")]
        //public async Task<IActionResult> GetHeatmapScatterData()
        //{
        //    var result = await _mediator.Send(new GetHeatmapScatterDataQuery());
        //    return Ok(result);
        //}
        [HttpGet("latest-alerts")]
        public async Task<IActionResult> GetLatestAlerts([FromQuery] int count = 10)
        {
            var result = await _mediator.Send(new GetLatestAlertsDetailsQuery(count));
            return Ok(result);
        }
        [HttpGet("bait-consumption-details")]
        public async Task<IActionResult> GetBaitConsumptionDetails(
    [FromQuery] string? groupNumber,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetBaitConsumptionDetailsQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }

        [HttpGet("total-bait-consumption")]
        public async Task<IActionResult> GetTotalBaitConsumption()
        {
            var total = await _mediator.Send(new GetTotalBaitConsumptionQuery());
            return Ok(new { Total = total });
        }
        [HttpGet("visit-pattern-details")]
        public async Task<IActionResult> GetVisitPatternDetails(
    [FromQuery] string? groupNumber,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetVisitPatternDetailsQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }

        [HttpGet("total-visits")]
        public async Task<IActionResult> GetTotalVisits()
        {
            var total = await _mediator.Send(new GetTotalVisitsQuery());
            return Ok(new { Total = total });
        }
        [HttpGet("average-severity-all-traps")]
        public async Task<IActionResult> GetAverageSeverityForAllTraps(
    [FromQuery] string? groupNumber,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetAverageSeverityForAllTrapsQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }
        [HttpGet("combined-severity")]
        public async Task<IActionResult> GetCombinedSeverity(
    [FromQuery] string? groupNumber,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetCombinedSeverityScoreQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }
        [HttpGet("average-severity-all")]
        public async Task<IActionResult> GetAverageSeverityAll(
    [FromQuery] string? groupNumber,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetAllTrapsAverageSeverityQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }


        [HttpGet("distinct-groups")]
        public async Task<IActionResult> GetDistinctGroups()
        {
            var result = await _mediator.Send(new GetDistinctGroupNumbersQuery());
            return Ok(result);
        }
        [HttpGet("traps-map-data")]
        public async Task<IActionResult> GetTrapsMapData([FromQuery] string? groupNumber)
        {
            var result = await _mediator.Send(new GetTrapsMapDataQuery(groupNumber));
            return Ok(result);
        }
        [HttpGet("monthly-visits-summary")]
        public async Task<IActionResult> GetMonthlyVisitsSummary(
    [FromQuery] string? groupNumber,
    [FromQuery] int? monthOffset = 0)
        {
            var result = await _mediator.Send(new GetMonthlyVisitsSummaryQuery(groupNumber, monthOffset));
            return Ok(result);
        }

        [HttpGet("daily-visits")]
        public async Task<IActionResult> GetDailyVisits(
            [FromQuery] string? groupNumber,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? days = 30)
        {
            var result = await _mediator.Send(new GetDailyVisitsQuery(groupNumber, fromDate, toDate, days));
            return Ok(result);



        }
        [HttpGet("average-daily-visits-summary")]
        public async Task<IActionResult> GetAverageDailyVisitsSummary(
    [FromQuery] string? groupNumber,
    [FromQuery] int? days = 30)
        {
            var result = await _mediator.Send(new GetAverageDailyVisitsSummaryQuery(groupNumber, days));
            return Ok(result);
        }
        [HttpGet("activity-by-hour-summary")]
        public async Task<IActionResult> GetActivityByHourSummary(
    [FromQuery] string? groupNumber,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetActivityByHourSummaryQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }

        [HttpGet("activity-by-hour-details")]
        public async Task<IActionResult> GetActivityByHourDetails(
            [FromQuery] string? groupNumber,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetActivityByHourDetailsQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }

        [HttpGet("hourly-activity")]
        public async Task<IActionResult> GetHourlyActivity(
            [FromQuery] string? groupNumber,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetHourlyActivityQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }
        [HttpGet("peak-hour-summary")]
        public async Task<IActionResult> GetPeakHourSummary(
    [FromQuery] string? groupNumber,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetPeakHourSummaryQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }

        [HttpGet("peak-hour-details")]
        public async Task<IActionResult> GetPeakHourDetails(
            [FromQuery] string? groupNumber,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await _mediator.Send(new GetPeakHourDetailsQuery(groupNumber, fromDate, toDate));
            return Ok(result);
        }
        [HttpGet("activity-index-by-location")]
        public async Task<IActionResult> GetActivityIndexByLocation([FromQuery] string? groupNumber)
        {
            var result = await _mediator.Send(new GetActivityIndexByLocationQuery(groupNumber));
            return Ok(result);
        }
        [HttpGet("activity-index-with-badges")]
        public async Task<IActionResult> GetActivityIndexWithBadges([FromQuery] string? groupNumber)
        {
            var result = await _mediator.Send(new GetActivityIndexWithBadgesQuery(groupNumber));
            return Ok(result);
        }

        [HttpGet("active-today")]
        public async Task<IActionResult> GetActiveTrapsToday([FromQuery] DateTime? date, [FromQuery] string? groupNumber)
        {
            var result = await _mediator.Send(new GetActiveTrapsTodayQuery(date, groupNumber));
            return Ok(result);
        }
    }
}
