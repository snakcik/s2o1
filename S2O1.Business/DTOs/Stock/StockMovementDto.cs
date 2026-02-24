using S2O1.Domain.Enums;
using FluentValidation;
using System;

namespace S2O1.Business.DTOs.Stock
{
    public class StockMovementDto
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int? TargetWarehouseId { get; set; }
        public MovementType MovementType { get; set; }
        public decimal Quantity { get; set; }
        public string DocumentNo { get; set; }
        public string? DocumentPath { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; } // Current User
        public int? SupplierId { get; set; }
        public int? CustomerId { get; set; }
    }

    public class StockMovementValidator : AbstractValidator<StockMovementDto>
    {
        public StockMovementValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.WarehouseId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
            
            RuleFor(x => x.TargetWarehouseId)
                .NotEmpty()
                .When(x => x.MovementType == MovementType.Transfer)
                .WithMessage("Target Warehouse is required for Transfer.");

            RuleFor(x => x.SupplierId)
                .NotEmpty()
                .When(x => x.MovementType == MovementType.Entry)
                .WithMessage("Supplier is required for Entry.");

            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .When(x => x.MovementType == MovementType.Exit)
                .WithMessage("Customer is required for Exit.");
        }
    }

    public class WaybillDto
    {
        public string WaybillNo { get; set; }
        public DateTime Date { get; set; }
        public string SupplierName { get; set; }
        public string Description { get; set; }
        public string DocumentPath { get; set; }
        public decimal TotalQuantity { get; set; }
    }
    public class WaybillItemDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string UnitName { get; set; }
        public string Description { get; set; }
    }
}
