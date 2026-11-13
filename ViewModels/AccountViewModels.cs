using System.ComponentModel.DataAnnotations;
namespace eGradjaninFBiH.ViewModels;
public class RegisterViewModel { [Required, StringLength(120)] public string FullName {get;set;}=""; [Required,EmailAddress] public string Email {get;set;}=""; [Required, StringLength(100,MinimumLength=8),DataType(DataType.Password)] public string Password {get;set;}=""; [Required,Compare(nameof(Password)),DataType(DataType.Password)] public string ConfirmPassword {get;set;}=""; }
public class LoginViewModel { [Required,EmailAddress] public string Email {get;set;}=""; [Required,DataType(DataType.Password)] public string Password {get;set;}=""; public bool RememberMe {get;set;} }
