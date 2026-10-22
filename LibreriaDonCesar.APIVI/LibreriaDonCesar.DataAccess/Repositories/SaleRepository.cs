using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LibreriaDonCesar.DataAccess.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly string _connectionString;

        //Constructor del repositorio
        public SaleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetAllAsync(PaginationParams pagination)
        {
            var sales = new List<SaleTransaction>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllSale", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PageNumber", pagination.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pagination.PageSize);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int totalRecords = 0;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 1. Primer resultado: las cabeceras de la página (el detalle se consulta aparte por Id)
                        while (await reader.ReadAsync())
                        {
                            sales.Add(new SaleTransaction
                            {
                                Master = new Sale
                                {
                                    Id = (int)reader["Id"],
                                    SaleDate = (DateTime)reader["SaleDate"],
                                    CustomerTypeId = (int)reader["CustomerTypeId"],
                                    CustomerTypeName = reader["CustomerTypeName"].ToString(),
                                    TotalAmount = (decimal)reader["TotalAmount"]
                                },
                                Details = new List<SaleDetail>()
                            });
                        }

                        // 2. Segundo resultado: el total de registros para calcular páginas
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                        {
                            totalRecords = reader.GetInt32(0);
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<SaleTransaction>>
                    {
                        Data = sales,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalRecords = totalRecords
                    };
                    response.OperationStatusCode = returnedValue;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<SaleTransaction>> GetByIdAsync(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetSaleById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", id);

                    var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
                    returnParam.Direction = ParameterDirection.ReturnValue;

                    SaleTransaction? transaction = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            transaction = new SaleTransaction
                            {
                                Master = new Sale
                                {
                                    Id = (int)reader["Id"],
                                    SaleDate = (DateTime)reader["SaleDate"],
                                    CustomerTypeId = (int)reader["CustomerTypeId"],
                                    CustomerTypeName = reader["CustomerTypeName"].ToString(),
                                    TotalAmount = (decimal)reader["TotalAmount"]
                                },
                                Details = new List<SaleDetail>()
                            };
                        }

                        if (transaction != null)
                        {
                            await reader.NextResultAsync();
                            var detailsList = new List<SaleDetail>();
                            while (await reader.ReadAsync())
                            {
                                detailsList.Add(new SaleDetail
                                {
                                    Id = (int)reader["Id"],
                                    SaleId = (int)reader["SaleId"],
                                    ProductId = (int)reader["ProductId"],
                                    ProductName = reader["ProductName"].ToString(),
                                    ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                    PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                    Quantity = (int)reader["Quantity"],
                                    SalePrice = (decimal)reader["SalePrice"],
                                    LineTotal = (decimal)reader["LineTotal"]
                                });
                            }
                            transaction.Details = detailsList;
                        }
                    }

                    var returnedValue = Convert.ToInt32(returnParam.Value);

                    return new RepositoryResponse<SaleTransaction>
                    {
                        Data = transaction,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }


        public async Task<RepositoryResponse<SaleTransaction>> InsertAsync(Sale master, IEnumerable<SaleDetail> details)
        {
            SaleTransaction? transaction = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_InsertSale", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SaleDate", master.SaleDate);
                    cmd.Parameters.AddWithValue("@CustomerTypeId", master.CustomerTypeId);

                    var detailsTable = new DataTable();
                    detailsTable.Columns.Add("ProductId", typeof(int));
                    detailsTable.Columns.Add("Quantity", typeof(int));

                    foreach (var item in details)
                    {
                        detailsTable.Rows.Add(item.ProductId, item.Quantity);
                    }

                    SqlParameter detailsParam = cmd.Parameters.AddWithValue("@SaleDetails", detailsTable);
                    detailsParam.SqlDbType = SqlDbType.Structured;
                    detailsParam.TypeName = "dbo.SaleDetailType";

                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 1) la venta
                        if (await reader.ReadAsync())
                        {
                            transaction = new SaleTransaction();
                            transaction.Master = new Sale
                            {
                                Id = (int)reader["Id"],
                                SaleDate = (DateTime)reader["SaleDate"],
                                CustomerTypeId = (int)reader["CustomerTypeId"],
                                CustomerTypeName = reader["CustomerTypeName"].ToString(),
                                TotalAmount = (decimal)reader["TotalAmount"]
                            };
                        }

                        if (transaction != null)
                        {
                            // 2) el detalle
                            var detailsList = new List<SaleDetail>();

                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    detailsList.Add(new SaleDetail
                                    {
                                        Id = (int)reader["Id"],
                                        SaleId = (int)reader["SaleId"],
                                        ProductId = (int)reader["ProductId"],
                                        ProductName = reader["ProductName"].ToString(),
                                        ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                        PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                        Quantity = (int)reader["Quantity"],
                                        SalePrice = (decimal)reader["SalePrice"],
                                        LineTotal = (decimal)reader["LineTotal"]
                                    });
                                }
                            }
                            transaction.Details = detailsList;

                            // 3) alertas de stock bajo de los productos vendidos
                            var alertsList = new List<StockAlert>();

                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    alertsList.Add(new StockAlert
                                    {
                                        ProductId = (int)reader["ProductId"],
                                        ProductName = reader["ProductName"].ToString(),
                                        UnitsInStock = (int)reader["UnitsInStock"],
                                        MinStock = (int)reader["MinStock"],
                                        StockStatus = reader["StockStatus"].ToString()
                                    });
                                }
                            }
                            transaction.Alerts = alertsList;
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<SaleTransaction>
                    {
                        Data = transaction,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<SaleDetail>>> GetDetailByIdAsync(int saleId)
        {
            var detailReturned = new List<SaleDetail>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetDetailsBySaleId", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SaleId", saleId);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var detail = new SaleDetail
                            {
                                Id = (int)reader["Id"],
                                SaleId = (int)reader["SaleId"],
                                ProductId = (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                Quantity = (int)reader["Quantity"],
                                SalePrice = (decimal)reader["SalePrice"],
                                LineTotal = (decimal)reader["LineTotal"]
                            };
                            detailReturned.Add(detail);
                        }
                    }

                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<IEnumerable<SaleDetail>>
                    {
                        Data = detailReturned,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<IEnumerable<SaleDetail>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<SaleDetail>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetSaleByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination)
        {
            var sales = new List<SaleTransaction>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_SaleByDateRange", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@PageNumber", pagination.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pagination.PageSize);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int totalRecords = 0;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 1. Primer resultado: las cabeceras de la página
                        while (await reader.ReadAsync())
                        {
                            sales.Add(new SaleTransaction
                            {
                                Master = new Sale
                                {
                                    Id = (int)reader["Id"],
                                    SaleDate = (DateTime)reader["SaleDate"],
                                    CustomerTypeId = (int)reader["CustomerTypeId"],
                                    CustomerTypeName = reader["CustomerTypeName"].ToString(),
                                    TotalAmount = (decimal)reader["TotalAmount"]
                                },
                                Details = new List<SaleDetail>()
                            });
                        }

                        // 2. Segundo resultado: el total de registros para calcular páginas
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                        {
                            totalRecords = reader.GetInt32(0);
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<SaleTransaction>>
                    {
                        Data = sales,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalRecords = totalRecords
                    };
                    response.OperationStatusCode = returnedValue;

                    return response;
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
