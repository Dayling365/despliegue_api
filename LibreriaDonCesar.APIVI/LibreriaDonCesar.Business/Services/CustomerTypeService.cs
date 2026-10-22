using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.Services
{
    public class CustomerTypeService : ICustomerTypeService
    {
        private readonly ICustomerTypeRepository _customerTypeRepository;

        public CustomerTypeService(ICustomerTypeRepository customerTypeRepository)
        {
            _customerTypeRepository = customerTypeRepository;
        }

        public async Task<ServiceResponse<IEnumerable<CustomerType>>> GetAllAsync()
        {
            var result = await _customerTypeRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                // el SP devuelve 0 aunque no haya filas: sin tipos de cliente = sin registros
                if (result.Data == null || !result.Data.Any())
                {
                    return new ServiceResponse<IEnumerable<CustomerType>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No hay tipos de cliente registrados"
                    };
                }

                return new ServiceResponse<IEnumerable<CustomerType>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            return new ServiceResponse<IEnumerable<CustomerType>>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = result.Message ?? "Ocurrió un error inesperado"
            };
        }
    }
}
