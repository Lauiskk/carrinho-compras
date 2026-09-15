namespace CarrinhoCompras.Api.ErrorHandling;

/// <summary>
/// Título, detalhe e código padrão por status HTTP. Garante que até erros gerados pelo próprio framework
/// (rota inexistente, método não permitido, tipo de conteúdo errado) sigam o mesmo formato e idioma.
/// </summary>
internal static class CodigosErroHttp
{
    public const string RequisicaoInvalida = "requisicao.invalida";

    public static string Titulo(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Requisição inválida",
        StatusCodes.Status404NotFound => "Recurso não encontrado",
        StatusCodes.Status405MethodNotAllowed => "Método não permitido",
        StatusCodes.Status409Conflict => "Conflito com o estado atual",
        StatusCodes.Status413PayloadTooLarge => "Requisição muito grande",
        StatusCodes.Status415UnsupportedMediaType => "Tipo de conteúdo não suportado",
        StatusCodes.Status422UnprocessableEntity => "Regra de negócio violada",
        >= StatusCodes.Status500InternalServerError => "Erro interno",
        _ => "Erro na requisição",
    };

    public static string Detalhe(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "A requisição é inválida.",
        StatusCodes.Status404NotFound => "O recurso solicitado não existe.",
        StatusCodes.Status405MethodNotAllowed => "Este recurso não aceita o método HTTP utilizado.",
        StatusCodes.Status413PayloadTooLarge => "O corpo da requisição excede o tamanho permitido.",
        StatusCodes.Status415UnsupportedMediaType => "Envie o corpo da requisição em JSON (Content-Type: application/json).",
        >= StatusCodes.Status500InternalServerError => "Ocorreu um erro inesperado. Tente novamente mais tarde.",
        _ => "Não foi possível processar a requisição.",
    };

    public static string Codigo(int status) => status switch
    {
        StatusCodes.Status400BadRequest => RequisicaoInvalida,
        StatusCodes.Status404NotFound => "recurso.nao_encontrado",
        StatusCodes.Status405MethodNotAllowed => "metodo.nao_permitido",
        StatusCodes.Status409Conflict => "conflito",
        StatusCodes.Status413PayloadTooLarge => "requisicao.muito_grande",
        StatusCodes.Status415UnsupportedMediaType => "midia.nao_suportada",
        StatusCodes.Status422UnprocessableEntity => "regra_negocio.violada",
        >= StatusCodes.Status500InternalServerError => "erro.interno",
        _ => "erro",
    };
}
