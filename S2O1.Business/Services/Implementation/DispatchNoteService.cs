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

        public async Task<IEnumerable<DispatchNoteDto>> GetAllAsync()
        {
            var data = await _unitOfWork.Repository<DispatchNote>().Query()
                .Include(d => d.Items).ThenInclude(i => i.Product)
                .Include(d => d.Company)
                .Include(d => d.Customer)
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();
                
            return _mapper.Map<IEnumerable<DispatchNoteDto>>(data);
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
