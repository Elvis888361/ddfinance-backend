using InsuranceAPI.Data;
using InsuranceAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace InsuranceAPI.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly ApplicationDbContext _context;

        public PolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Policy>> GetAllPoliciesAsync(string? search = null, string? type = null, string? sortBy = null)
        {
            IQueryable<Policy> query = _context.Policies;

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p => 
                    p.PolicyNumber.ToLower().Contains(search) || 
                    p.HolderName.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<PolicyType>(type, true, out var policyType))
            {
                query = query.Where(p => p.Type == policyType);
            }

            query = sortBy?.ToLower() switch
            {
                "date" => query.OrderByDescending(p => p.StartDate),
                "premium" => query.OrderByDescending(p => p.Premium),
                "name" => query.OrderBy(p => p.HolderName),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return await query.ToListAsync();
        }

        public async Task<Policy?> GetPolicyByIdAsync(int id)
        {
            return await _context.Policies.FindAsync(id);
        }

        public async Task<Policy> CreatePolicyAsync(Policy policy)
        {
            try
            {
                // Validate policy number format
                if (!Regex.IsMatch(policy.PolicyNumber, @"^[A-Z0-9]{8,}$"))
                {
                    throw new ArgumentException("Policy number must be at least 8 characters and contain only uppercase letters and numbers");
                }

                // Validate dates
                if (policy.EndDate <= policy.StartDate)
                {
                    throw new ArgumentException("End date must be after start date");
                }

                // Validate premium
                if (policy.Premium <= 0)
                {
                    throw new ArgumentException("Premium must be greater than 0");
                }

                // Check for duplicate policy number
                var existingPolicy = await _context.Policies
                    .FirstOrDefaultAsync(p => p.PolicyNumber == policy.PolicyNumber);
                
                if (existingPolicy != null)
                {
                    throw new ArgumentException("Policy number already exists");
                }

                policy.CreatedAt = DateTime.UtcNow;
                policy.UpdatedAt = DateTime.UtcNow;

                await _context.Policies.AddAsync(policy);
                await _context.SaveChangesAsync();

                return policy;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating policy: {ex.Message}", ex);
            }
        }

        public async Task<Policy?> UpdatePolicyAsync(int id, Policy policy)
        {
            try
            {
                var existingPolicy = await _context.Policies.FindAsync(id);

                if (existingPolicy == null)
                    return null;

                // Validate dates
                if (policy.EndDate <= policy.StartDate)
                {
                    throw new ArgumentException("End date must be after start date");
                }

                // Validate premium
                if (policy.Premium <= 0)
                {
                    throw new ArgumentException("Premium must be greater than 0");
                }

                // Update fields
                existingPolicy.HolderName = policy.HolderName;
                existingPolicy.Type = policy.Type;
                existingPolicy.StartDate = policy.StartDate;
                existingPolicy.EndDate = policy.EndDate;
                existingPolicy.Premium = policy.Premium;
                existingPolicy.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return existingPolicy;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating policy: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeletePolicyAsync(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null)
                return false;

            _context.Policies.Remove(policy);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 