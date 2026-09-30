using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Azure;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LibreriaDonCesar.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        //Cmpo global para capturar la cadena de conexión a la base de datos    
        private readonly string _connectionString;

        //Constructor del repositorio
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        public async Task<RepositoryResponse<UserRole>> AssignRoleAsync(int userId, int roleId)
        {
            var UserRoleReturned = new UserRole();
            var response = new RepositoryResponse<UserRole>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_AssignRoleUser", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@RoleId", roleId);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    await cmd.ExecuteNonQueryAsync();


                    UserRoleReturned.UserId = userId;
                    UserRoleReturned.RoleId = roleId;

                    response.Data = UserRoleReturned;
                    response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return response;


                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<UserRole>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message

                };

            }

            ///
            catch (Exception ex)
            {
                return new RepositoryResponse<UserRole>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }









        }
        public async Task<RepositoryResponse<User>> AddAsync(User user)
        {
            var userReturned = new User();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_InsertNewUser", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@Email", user.Email);

                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userReturned.Id = (int)reader["Id"];
                            userReturned.UserName = reader["UserName"].ToString();
                            userReturned.PasswordHash = reader["PasswordHash"].ToString();
                            userReturned.Email = reader["Email"].ToString();
                            userReturned.State = (bool)reader["State"];


                        }
                    }

                    //capturamos el codigo de retorno
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<User>
                    {
                        Data = userReturned,
                        OperationStatusCode = returnedValue

                    };


                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<User>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message

                };

            }

            ///
            catch (Exception ex)
            {
                return new RepositoryResponse<User>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        //Metodo para obtener una lista de categorias, y retorna un objeto de tipo RepositoryResponse
        public async Task<RepositoryResponse<PagedResponse<IEnumerable<User>>>> GetAllAsync(PaginationParams pagination)
        {
            var users = new List<User>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<User>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllUsers", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PageNumber", pagination.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pagination.PageSize);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int totalRecords = 0;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int userId = (int)reader["Id"];

                            var existingUser = users.FirstOrDefault(u => u.Id == userId);

                            if (existingUser == null)
                            {
                                existingUser = new User
                                {
                                    Id = userId,
                                    UserName = reader["UserName"].ToString()!,
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    State = reader["State"] != DBNull.Value ? (bool)reader["State"] : false,
                                    Roles = new List<string>() 
                                };
                                users.Add(existingUser);
                            }

                            if (reader["RoleName"] != DBNull.Value)
                            {
                                var roleName = reader["RoleName"].ToString();
                                if (!string.IsNullOrEmpty(roleName) && !existingUser.Roles.Contains(roleName))
                                {
                                    existingUser.Roles.Add(roleName);
                                }
                            }
                        }

                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                            totalRecords = reader.GetInt32(0);
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<User>>
                    {
                        Data = users,
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



        public async Task<RepositoryResponse<User>> GetByIdAsync(int id)
        {
            var userReturned = new User();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_GetUserById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (userReturned.Id==0)
                            {
                                userReturned.Id = (int)reader["Id"];
                                userReturned.UserName = reader["UserName"].ToString();
                                userReturned.PasswordHash = reader["PasswordHash"].ToString();
                                userReturned.Email = reader["Email"].ToString();
                                userReturned.State = (bool)reader["State"];

                            }

                            if (reader["RoleName"] != DBNull.Value)
                            {
                                var roleName = reader["RoleName"].ToString();
                                if (!string.IsNullOrEmpty(roleName) && !userReturned.Roles.Contains(roleName))
                                {
                                    userReturned.Roles.Add(roleName);
                                }
                            }
                        }
                    }

                    //capturamos el codigo de retorno
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<User>
                    {
                        Data = userReturned,
                        OperationStatusCode = returnedValue

                    };


                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<User>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message

                };

            }

            catch (Exception ex)
            {
                return new RepositoryResponse<User>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }




        public async Task<RepositoryResponse<User>> GetByNameAsync(string name)
        {
            var User = new User();
            var response = new RepositoryResponse<User>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUsersByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UsersName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        User.Roles = new List<string>();

                        while (await reader.ReadAsync())
                        {

                            if (User.Id == 0)
                            {
                                User.Id = (int)reader["UserId"];
                                User.UserName = reader["UserName"].ToString();
                                User.PasswordHash = reader["PasswordHash"].ToString();
                                User.Email = reader["Email"].ToString();
                                User.State = (bool)reader["State"];
                            }

                            // Agregar roles si existen
                            if (reader["RoleName"] != DBNull.Value)
                            {
                                var roleName = reader["RoleName"].ToString();
                                if (!User.Roles.Contains(roleName))
                                    User.Roles.Add(roleName);
                            }
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = User;
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

            ////
            catch (Exception ex)
            {
                return new RepositoryResponse<User>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }


        public async Task<RepositoryResponse<User>> UpdateAsync(int id, User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateUser", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

                    cmd.Parameters.AddWithValue("@State", user.State);




                    User userUpdated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userUpdated = new User
                            {
                                Id = (int)reader["Id"],
                                UserName = reader["UserName"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Email = reader["Email"].ToString(),
                                State = (bool)reader["State"]
                            };
                        }
                    }

                    return new RepositoryResponse<User>
                    {
                        Data = userUpdated, ////me///
                        OperationStatusCode = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<User>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }


        public async Task<RepositoryResponse<User>> SetStateAsync(int userId, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateUserState", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", userId);
                    cmd.Parameters.AddWithValue("@State", state);

                    User userUpdated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userUpdated = new User
                            {
                                Id = (int)reader["Id"],
                                UserName = reader["UserName"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Email = reader["Email"].ToString(),
                                State = (bool)reader["State"]
                            };
                        }
                    }

                    return new RepositoryResponse<User>
                    {
                        Data = userUpdated,
                        OperationStatusCode = userUpdated != null ? 0 : 1
                    };
                }
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<User>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

    }


















}