using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;

namespace LibreriaDonCesar.Business.Services
{
    public class InvoiceService:IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        public InvoiceService(IInvoiceRepository invoice)
        {
            _invoiceRepository = invoice;
        }

        public async Task<ServiceResponse<List<Invoice>>> InvoiceQueue()
        {
            try
            {
                var repositoryResponse = await _invoiceRepository.GetInvoiceQueue();
                //validar el codigo de operacion si trae regidtros si la can es 0 si trae lo pasamos
                //ala lista si no trae mandamos a decir q la cola viene vacia si ocurrio algo mandamos ala respu
                if (repositoryResponse.OperationStatusCode == 0)
                {
                    if (repositoryResponse.Data!.Count > 0)
                    {
                        return new ServiceResponse<List<Invoice>>
                        {
                            //conversion de la cola a una coleccion tipo lista
                            Data = repositoryResponse.Data.ToList(),
                            IsSuccess = true,
                            Message = "Operacion exitosa",
                            MessageCode = MessageCodes.Success
                        };
                    }
                    else
                    {
                        return new ServiceResponse<List<Invoice>>
                        {
                            Data = null,
                            IsSuccess = false,
                            Message = "No hay facturas que imprimir",
                            MessageCode = MessageCodes.NoData
                        };
                    }


                }
                return new ServiceResponse<List<Invoice>>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Ocurrio un erro inesperado",
                    MessageCode = MessageCodes.ErrorDataBase
                };
            }
            catch (Exception )
            {
                return new ServiceResponse<List<Invoice>>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Ocurrio un error inesperado",
                    MessageCode = MessageCodes.ErrorDataBase
                };
            }
        }

        public async Task<ServiceResponse<Invoice>> ToPrint()
        {
            try
            {
            var FillQueue = await _invoiceRepository.GetInvoiceQueue();
                if (FillQueue.Data!.Count == 0)
                {
                    return new ServiceResponse<Invoice>
                    {
                        Data = null,
                        IsSuccess = false,
                        Message = "No hay nada por imprimir",
                        MessageCode = MessageCodes.NoData

                    };
                }

                var repoResponse = await _invoiceRepository.ToPrint();

                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Invoice>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Factura impresa exitosamente"
                    };
                }

                // No hay facturas que imprimir
                if (repoResponse.OperationStatusCode == 1)
                {
                    return new ServiceResponse<Invoice>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "No hay facturas para imprimir"
                    };
                }

                // Error en base de datos
                return new ServiceResponse<Invoice>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Invoice>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = ex.Message
                };
            }
        }
    }
}
