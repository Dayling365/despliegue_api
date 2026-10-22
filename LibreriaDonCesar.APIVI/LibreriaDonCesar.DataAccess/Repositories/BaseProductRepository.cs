using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Repositories
{
    public class BaseProductRepository : IBaseProductRepository
    {
        private readonly string _connectionString;

        public BaseProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PagedResponse<IEnumerable<BaseProduct>>>> GetAllAsync(PaginationParams pagination)
        {
            var baseProducts = new List<BaseProduct>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<BaseProduct>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllBaseProducts", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PageNumber", pagination.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pagination.PageSize);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int totalRecords = 0;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // primer resultado: los registros de la página
                        while (await reader.ReadAsync())
                        {
                            baseProducts.Add(new BaseProduct
                            {
                                Id = (int)reader["Id"],
                                ProductName = reader["ProductName"].ToString()!,
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                CategoryId = (int)reader["CategoryId"],
                                CategoryName = reader["CategoryName"].ToString(),
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString(),
                                State = reader["State"] != DBNull.Value ? (bool)reader["State"] : false
                            });
                        }

                        // segundo resultado: el total de registros para calcular páginas
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                            totalRecords = reader.GetInt32(0);
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<BaseProduct>>
                    {
                        Data = baseProducts,
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

        public async Task<RepositoryResponse<BaseProduct>> GetByIdAsync(int id)
        {
            BaseProduct? baseProductReturned = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetBaseProductById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            baseProductReturned = new BaseProduct();
                            baseProductReturned.Id = (int)reader["Id"];
                            baseProductReturned.ProductName = reader["ProductName"].ToString()!;
                            baseProductReturned.Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null;
                            baseProductReturned.CategoryId = (int)reader["CategoryId"];
                            baseProductReturned.CategoryName = reader["CategoryName"].ToString();
                            baseProductReturned.BrandId = (int)reader["BrandId"];
                            baseProductReturned.BrandName = reader["BrandName"].ToString();
                            baseProductReturned.State = (bool)reader["State"];
                        }
                    }

                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<BaseProduct>
                    {
                        Data = baseProductReturned,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<BaseProduct>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<BaseProduct>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<BaseProduct>> AddAsync(BaseProduct baseProduct)
        {
            BaseProduct? baseProductReturned = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_InsertNewBaseProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductName", baseProduct.ProductName);
                    cmd.Parameters.AddWithValue("@Description", (object?)baseProduct.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", baseProduct.CategoryId);
                    cmd.Parameters.AddWithValue("@BrandId", baseProduct.BrandId);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // OUTPUT INSERTED.* de la tabla BaseProducts
                            baseProductReturned = new BaseProduct();
                            baseProductReturned.Id = (int)reader["Id"];
                            baseProductReturned.ProductName = reader["ProductName"].ToString()!;
                            baseProductReturned.Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null;
                            baseProductReturned.CategoryId = (int)reader["CategoryId"];
                            baseProductReturned.BrandId = (int)reader["BrandId"];
                            baseProductReturned.State = (bool)reader["State"];
                        }
                    }

                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<BaseProduct>
                    {
                        Data = baseProductReturned,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<BaseProduct>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<BaseProduct>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<BaseProduct>> UpdateAsync(int id, BaseProduct baseProduct)
        {
            BaseProduct? baseProductUpdated = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateBaseProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ProductName", baseProduct.ProductName);
                    cmd.Parameters.AddWithValue("@Description", (object?)baseProduct.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", baseProduct.CategoryId);
                    cmd.Parameters.AddWithValue("@BrandId", baseProduct.BrandId);
                    cmd.Parameters.AddWithValue("@State", baseProduct.State);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            baseProductUpdated = new BaseProduct();
                            baseProductUpdated.Id = (int)reader["Id"];
                            baseProductUpdated.ProductName = reader["ProductName"].ToString()!;
                            baseProductUpdated.Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null;
                            baseProductUpdated.CategoryId = (int)reader["CategoryId"];
                            baseProductUpdated.BrandId = (int)reader["BrandId"];
                            baseProductUpdated.State = (bool)reader["State"];
                        }
                    }

                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<BaseProduct>
                    {
                        Data = baseProductUpdated,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<BaseProduct>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<BaseProduct>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<BaseProduct>> SetStateAsync(int id, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateBaseProductState", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@State", state);

                    BaseProduct? baseProductUpdated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // OUTPUT INSERTED.* de la tabla BaseProducts
                            baseProductUpdated = new BaseProduct
                            {
                                Id = (int)reader["Id"],
                                ProductName = reader["ProductName"].ToString()!,
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                CategoryId = (int)reader["CategoryId"],
                                BrandId = (int)reader["BrandId"],
                                State = (bool)reader["State"]
                            };
                        }
                    }

                    // este SP no usa RETURN: si no actualizó ninguna fila, la plantilla no existe (50050)
                    return new RepositoryResponse<BaseProduct>
                    {
                        Data = baseProductUpdated,
                        OperationStatusCode = baseProductUpdated != null ? 0 : 50050
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<BaseProduct>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<BaseProduct>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
