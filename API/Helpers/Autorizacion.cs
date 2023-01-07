namespace API.Helpers;

public class Autorizacion
{
	public enum Roles
	{
		Administrador,
		Gerente,
		Empleado
	}

	public const Roles ROL_PREDETERMINADO= Roles.Empleado;
}
