using AutoMapper;
using S2O1.Business.DTOs.Logistic;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace S2O1.Business.Services.Implementation
{
    public class DispatchNoteService : IDispatchNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public DispatchNoteService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<S2O1.Business.DTOs.Common.PagedResultDto<DispatchNoteDto>> GetAllAsync(string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Repository<DispatchNote>().Query()
                .Include(d => d.Items).ThenInclude(i => i.Product)
                .Include(d => d.Company)
                .Include(d => d.Customer)
                .Where(d => !d.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(x => x.DispatchNo.ToLower().Contains(search) || x.Customer.CustomerContactPersonName.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var data = await query.OrderByDescending(d => d.CreateDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            var mapped = _mapper.Map<IEnumerable<DispatchNoteDto>>(data);

            return new S2O1.Business.DTOs.Common.PagedResultDto<DispatchNoteDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<DispatchNoteDto> GetByIdAsync(int id)
        {
            var data = await _unitOfWork.Repository<DispatchNote>().Query()
                .Include(d => d.Items).ThenInclude(i => i.Product)
                .Include(d => d.Company)
                .Include(d => d.Customer)
                .FirstOrDefaultAsync(d => d.Id == id);
                
            return _mapper.Map<DispatchNoteDto>(data);
        }

        public async Task<DispatchNoteDto> CreateAsync(CreateDispatchNoteDto dto)
        {
            var entity = _mapper.Map<DispatchNote>(dto);
            entity.CreateDate = System.DateTime.Now;
            entity.Status = "Hazırlanıyor"; // Initial status
            
            await _unitOfWork.Repository<DispatchNote>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            return _mapper.Map<DispatchNoteDto>(entity);
        }
        
        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
             var entity = await _unitOfWork.Repository<DispatchNote>().GetByIdAsync(id);
             if (entity == null) return false;
             
             entity.Status = status;
             _unitOfWork.Repository<DispatchNote>().Update(entity);
             await _unitOfWork.SaveChangesAsync();
             return true;
        }
    }
}
