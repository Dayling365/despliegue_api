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
    public class ProductRepository: IProductRepository
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
                                CategoryId = (int)reader["CategoryId"],
                                CategoryName = reader["CategoryName"].ToString()!,
                                PresentationId = (int)reader["PresentationId"],
                                PresentationName = reader["PresentationName"].ToString()!,
                                ProductName = reader["ProductName"].ToString(),
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString()!,
                                ColorId = (int)reader["ColorId"],
                                ColorName = reader["ColorName"].ToString()!,
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                Description = reader["Description"].ToString(),
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
            }
            return response;
        }

        public async Task<RepositoryResponse<Product>> GetByIdAsync(int id)
        {
            var productReturned = new Product();
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
                            productReturned.Id = (int)reader["Id"];
                            productReturned.CategoryId = (int)reader["CategoryId"];
                            productReturned.CategoryName=reader["CategoryName"].ToString();
                            productReturned.PresentationId = (int)reader["PresentationId"];
                            productReturned.PresentationName = reader["PresentationName"].ToString();
                            productReturned.ProductName = reader["ProductName"].ToString();
                            productReturned.BrandId = (int)reader["BrandId"];
                            productReturned.BrandName = reader["BrandName"].ToString();
                            productReturned.ColorId = (int)reader["ColorId"];
                            productReturned.ColorName = reader["ColorName"].ToString();
                            productReturned.UnitPrice = Convert.ToDecimal(reader["UnitPrice"]);
                            productReturned.Description = reader["Description"].ToString();
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

        public async Task<RepositoryResponse<Product>> GetByNameAsync(string name)
        {
            var Product = new Product();
            var response = new RepositoryResponse<Product>();

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
                        if (await reader.ReadAsync())
                        {
                            Product.Id = (int)reader["Id"];
                            Product.CategoryId = (int)reader["CategoryId"];
                            Product.CategoryName = reader["CategoryName"].ToString();
                            Product.PresentationId = (int)reader["PresentationId"];
                            Product.ProductName = reader["ProductName"].ToString();
                            Product.BrandId = (int)reader["BrandId"];
                            Product.BrandName = reader["BrandName"].ToString();
                            Product.ColorId = (int)reader["ColorId"];
                            Product.ColorName = reader["ColorName"].ToString();
                            Product.UnitPrice = Convert.ToDecimal(reader["UnitPrice"]);
                            Product.Description = reader["Description"].ToString();
                            Product.State = (bool)reader["State"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = Product;
                    response.OperationStatusCode = returnedValue;

                    return response;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                return response;
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


        public async Task<RepositoryResponse<Product>> AddAsync(Product product)
        {
            var productReturned = new Product();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_InsertNewProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    cmd.Parameters.AddWithValue("@PresentationId", product.PresentationId);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@BrandId", product.BrandId);
                    cmd.Parameters.AddWithValue("@ColorId", product.ColorId);
                    cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    //cmd.Parameters.AddWithValue("@State", product.State);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            productReturned.Id = (int)reader["Id"];
                            productReturned.CategoryId = (int)reader["CategoryId"];
                            productReturned.PresentationId = (int)reader["PresentationId"];
                            productReturned.ProductName = reader["ProductName"].ToString();
                            productReturned.BrandId = (int)reader["BrandId"];  
                            productReturned.ColorId = (int)reader["ColorId"];
                            productReturned.UnitPrice = Convert.ToDecimal(reader["UnitPrice"]);
                            productReturned.Description = reader["Description"].ToString();
                            //productReturned.State = (bool)reader["State"];

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
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    cmd.Parameters.AddWithValue("@PresentationId", product.PresentationId);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@BrandId", product.BrandId);
                    cmd.Parameters.AddWithValue("@ColorId", product.ColorId);
                    cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@State", product.State);

                    Product productUpdated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            productUpdated = new Product
                            {
                                Id = (int)reader["Id"],
                                CategoryId = (int)reader["CategoryId"],
                                PresentationId = (int)reader["PresentationId"],
                                ProductName = reader["ProductName"].ToString(),
                                BrandId = (int)reader["BrandId"],
                                ColorId = (int)reader["ColorId"],
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                Description = reader["Description"].ToString(),
                                State = (bool)reader["State"],
                            };
                        }
                    }

                    return new RepositoryResponse<Product>
                    {
                        Data = productUpdated, 
                        OperationStatusCode = 0
                    };
                }
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

                    Product productUpdated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            productUpdated = new Product
                            {
                                Id = (int)reader["Id"],
                                CategoryId = (int)reader["CategoryId"],
                                PresentationId = (int)reader["PresentationId"],
                                ProductName = reader["ProductName"].ToString(),
                                BrandId = (int)reader["BrandId"],
                                ColorId = (int)reader["ColorId"],
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                Description = reader["Description"].ToString(),
                                State = (bool)reader["State"],
                            };
                        }
                    }

                    return new RepositoryResponse<Product>
                    {
                        Data = productUpdated,
                        OperationStatusCode = productUpdated != null ? 0 : 1
                    };
                }
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
