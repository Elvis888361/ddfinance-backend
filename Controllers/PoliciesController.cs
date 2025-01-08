using Microsoft.AspNetCore.Mvc;
using InsuranceAPI.Models;
using InsuranceAPI.Services;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _policyService;
        private readonly ILogger<PoliciesController> _logger;

        public PoliciesController(IPolicyService policyService, ILogger<PoliciesController> logger)
        {
            _policyService = policyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Policy>>> GetPolicies(
            [FromQuery] string? search,
            [FromQuery] string? type,
            [FromQuery] string? sortBy)
        {
            try
            {
                var policies = await _policyService.GetAllPoliciesAsync(search, type, sortBy);
                return Ok(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies");
                return StatusCode(500, "An error occurred while retrieving policies");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Policy>> GetPolicy(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);

            if (policy == null)
                return NotFound();

            return Ok(policy);
        }

        [HttpPost]
        public async Task<ActionResult<Policy>> CreatePolicy(Policy policy)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();
                    return BadRequest(new { Message = "Validation failed", Errors = errors });
                }

                var createdPolicy = await _policyService.CreatePolicyAsync(policy);
                return CreatedAtAction(nameof(GetPolicy), new { id = createdPolicy.Id }, createdPolicy);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid policy data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy");
                return StatusCode(500, new { Message = "An error occurred while creating the policy", Detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolicy(int id, Policy policy)
        {
            try
            {
                if (id != policy.Id)
                {
                    return BadRequest(new { Message = "Policy ID mismatch" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();
                    return BadRequest(new { Message = "Validation failed", Errors = errors });
                }

                var updatedPolicy = await _policyService.UpdatePolicyAsync(id, policy);

                if (updatedPolicy == null)
                {
                    return NotFound(new { Message = "Policy not found" });
                }

                return Ok(updatedPolicy);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid policy data during update: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy");
                return StatusCode(500, new { Message = "An error occurred while updating the policy", Detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var result = await _policyService.DeletePolicyAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
} 