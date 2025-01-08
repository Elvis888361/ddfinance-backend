using InsuranceAPI.Models;

namespace InsuranceAPI.Services
{
    public interface IPolicyService
    {
        Task<IEnumerable<Policy>> GetAllPoliciesAsync(string? search = null, string? type = null, string? sortBy = null);
        Task<Policy?> GetPolicyByIdAsync(int id);
        Task<Policy> CreatePolicyAsync(Policy policy);
        Task<Policy?> UpdatePolicyAsync(int id, Policy policy);
        Task<bool> DeletePolicyAsync(int id);
    }
} 