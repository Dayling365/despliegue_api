using System;
using System.Collections.Generic;
using System.Data;
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
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        //Constructor del repositorio
        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // GetByUserNameAsync    string username
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

        public async Task<RepositoryResponse<User>> GetByEmailAsync(string email)
        {
            var user = new User();
            var response = new RepositoryResponse<User>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_GetAllUsersByEmail", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user.Id = (int)reader["Id"];
                            user.UserName = reader["UserName"].ToString()!;
                            user.Email = reader["Email"].ToString()!;
                            user.PasswordHash = reader["PasswordHash"].ToString()!;
                            user.State = (bool)reader["State"];
                        }

                       
                    }

                    //capturar codigo de retorno
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = user;
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
                return new RepositoryResponse<User>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }


        }


        public async Task<RepositoryResponse<User>> RegisterAsync(User user)
        { 
            var userReturned = new User();
            var response = new RepositoryResponse<User>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_RegisterUser", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userReturned.Id = (int)reader["Id"];
                            userReturned.UserName = (string)reader["UserName"];
                            userReturned.Email = (string)reader["Email"];
                           // userReturned.PasswordHash = (string)reader["PasswordHash"];
                            userReturned.State = (bool)reader["State"];
                        }
                    }

                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);



                    response.Data = userReturned;
                    response.OperationStatusCode = retornedValue;
                    return response;
                    
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

       public async Task<RepositoryResponse<IEnumerable<string>>> GetRolesByUserIdAsync(int userId)
        {
            var roles = new List<string>();
            var response = new RepositoryResponse<IEnumerable<string>>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var cmd = new SqlCommand("USP_GetUserRolesByUserId", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            roles.Add(reader["RoleName"].ToString()!);
                        }
                    }
                    //capturar codigo de retorno
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = roles;
                    response.OperationStatusCode = returnedValue;
                    response.Message = "Operación exitosa";
                }


            }

            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
                 
            }

            return response;

        }

    }
}
