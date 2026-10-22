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
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PagedResponse<IEnumerable<Product>>>> GetAllAsync(PaginationParams pagination)
        {
            var products = new List<Product>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<Product>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllProducts", connection);
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
                            products.Add(new Product
                            {
                                Id = (int)reader["Id"],
                                BaseProductId = (int)reader["BaseProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                CategoryId = (int)reader["CategoryId"],
                                CategoryName = reader["CategoryName"].ToString(),
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString(),
                                ColorId = reader["ColorId"] != DBNull.Value ? (int)reader["ColorId"] : null,
                                ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                PresentationId = reader["PresentationId"] != DBNull.Value ? (int)reader["PresentationId"] : null,
                                PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                SalePrice = Convert.ToDecimal(reader["SalePrice"]),
                                State = reader["State"] != DBNull.Value ? (bool)reader["State"] : false
                            });
                        }

                        // segundo resultado: el total de registros para calcular páginas
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                            totalRecords = reader.GetInt32(0);
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<Product>>
                    {
                        Data = products,
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

        public async Task<RepositoryResponse<Product>> GetByIdAsync(int id)
        {
            Product? productReturned = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_GetProductById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            productReturned = new Product();
                            productReturned.Id = (int)reader["Id"];
                            productReturned.BaseProductId = (int)reader["BaseProductId"];
                            productReturned.ProductName = reader["ProductName"].ToString();
                            productReturned.Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null;
                            productReturned.CategoryId = (int)reader["CategoryId"];
                            productReturned.CategoryName = reader["CategoryName"].ToString();
                            productReturned.BrandId = (int)reader["BrandId"];
                            productReturned.BrandName = reader["BrandName"].ToString();
                            productReturned.ColorId = reader["ColorId"] != DBNull.Value ? (int)reader["ColorId"] : null;
                            productReturned.ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null;
                            productReturned.PresentationId = reader["PresentationId"] != DBNull.Value ? (int)reader["PresentationId"] : null;
                            productReturned.PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null;
                            productReturned.SalePrice = Convert.ToDecimal(reader["SalePrice"]);
                            productReturned.State = (bool)reader["State"];
                        }
                    }

                    //capturamos el codigo de retorno
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Product>
                    {
                        Data = productReturned,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Product>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Product>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        // Devuelve TODAS las variantes (color / presentación) del producto con ese nombre
        public async Task<RepositoryResponse<IEnumerable<Product>>> GetByNameAsync(string name)
        {
            var products = new List<Product>();
            var response = new RepositoryResponse<IEnumerable<Product>>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                Id = (int)reader["Id"],
                                BaseProductId = (int)reader["BaseProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                CategoryId = (int)reader["CategoryId"],
                                CategoryName = reader["CategoryName"].ToString(),
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString(),
                                ColorId = reader["ColorId"] != DBNull.Value ? (int)reader["ColorId"] : null,
                                ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                PresentationId = reader["PresentationId"] != DBNull.Value ? (int)reader["PresentationId"] : null,
                                PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                SalePrice = Convert.ToDecimal(reader["SalePrice"]),
                                State = (bool)reader["State"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = products;
                    response.OperationStatusCode = returnedValue;

                    return response;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
                return response;
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<Product>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }


        public async Task<RepositoryResponse<Product>> AddAsync(Product product)
        {
            Product? productReturned = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_InsertNewProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BaseProductId", product.BaseProductId);
                    cmd.Parameters.AddWithValue("@ColorId", (object?)product.ColorId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PresentationId", (object?)product.PresentationId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SalePrice", product.SalePrice);
                    cmd.Parameters.AddWithValue("@MinStock", product.MinStock);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // el SP devuelve la fila de la vista del catálogo (la columna del id se llama ProductId)
                            productReturned = new Product();
                            productReturned.Id = (int)reader["ProductId"];
                            productReturned.BaseProductId = (int)reader["BaseProductId"];
                            productReturned.ProductName = reader["ProductName"].ToString();
                            productReturned.Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null;
                            productReturned.CategoryId = (int)reader["CategoryId"];
                            productReturned.CategoryName = reader["CategoryName"].ToString();
                            productReturned.BrandId = (int)reader["BrandId"];
                            productReturned.BrandName = reader["BrandName"].ToString();
                            productReturned.ColorId = reader["ColorId"] != DBNull.Value ? (int)reader["ColorId"] : null;
                            productReturned.ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null;
                            productReturned.PresentationId = reader["PresentationId"] != DBNull.Value ? (int)reader["PresentationId"] : null;
                            productReturned.PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null;
                            productReturned.SalePrice = Convert.ToDecimal(reader["SalePrice"]);
                            productReturned.State = (bool)reader["State"];
                        }
                    }

                    //capturamos el codigo de retorno
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Product>
                    {
                        Data = productReturned,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Product>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Product>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Product>> UpdateAsync(int id, Product product)
        {
            Product? productUpdated = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@BaseProductId", product.BaseProductId);
                    cmd.Parameters.AddWithValue("@ColorId", (object?)product.ColorId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PresentationId", (object?)product.PresentationId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SalePrice", product.SalePrice);
                    cmd.Parameters.AddWithValue("@State", product.State);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // el SP devuelve la fila de la vista del catálogo (la columna del id se llama ProductId)
                            productUpdated = new Product
                            {
                                Id = (int)reader["ProductId"],
                                BaseProductId = (int)reader["BaseProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                CategoryId = (int)reader["CategoryId"],
                                CategoryName = reader["CategoryName"].ToString(),
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString(),
                                ColorId = reader["ColorId"] != DBNull.Value ? (int)reader["ColorId"] : null,
                                ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                PresentationId = reader["PresentationId"] != DBNull.Value ? (int)reader["PresentationId"] : null,
                                PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                SalePrice = Convert.ToDecimal(reader["SalePrice"]),
                                State = (bool)reader["State"]
                            };
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Product>
                    {
                        Data = productUpdated,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Product>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Product>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Product>> SetStateAsync(int productId, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateProductState", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", productId);
                    cmd.Parameters.AddWithValue("@State", state);

                    Product? productUpdated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // OUTPUT INSERTED.* de la tabla Products
                            productUpdated = new Product
                            {
                                Id = (int)reader["Id"],
                                BaseProductId = (int)reader["BaseProductId"],
                                ColorId = reader["ColorId"] != DBNull.Value ? (int)reader["ColorId"] : null,
                                PresentationId = reader["PresentationId"] != DBNull.Value ? (int)reader["PresentationId"] : null,
                                SalePrice = Convert.ToDecimal(reader["SalePrice"]),
                                State = (bool)reader["State"]
                            };
                        }
                    }

                    return new RepositoryResponse<Product>
                    {
                        Data = productUpdated,
                        OperationStatusCode = productUpdated != null ? 0 : 50050
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Product>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Product>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

    }
}
