// Constantes para valores padrão
export const DEFAULT_ERROR_TITLE = "Erro no Git";
export const DEFAULT_ERROR_DESCRIPTION = "Ocorreu um erro ao executar o comando Git.";

// Padrões de erro do Git
export const ERROR_PATTERNS = [
  {
    pattern: /Your configuration specifies to merge with the ref ['"]([^'"]+)['"][\s\S]*?but no such ref was fetched/i,
    title: "Branch não encontrada",
    description: (match) => {
      const branchName = match[1].replace('refs/heads/', '');
      return `A branch '${branchName}' não foi encontrada no repositório remoto. Verifique se o nome da branch está correto e se ela existe no servidor.`;
    }
  },
  {
    pattern: /fatal: unable to access.+Could not resolve host/i,
    title: "Erro de conexão",
    description: "Não foi possível conectar ao servidor Git. Verifique sua conexão com a internet e as configurações de DNS."
  },
  {
    pattern: /fatal: unable to access.+Failed to connect.+Connection timed out/i,
    title: "Tempo de conexão esgotado",
    description: "A conexão com o servidor Git expirou. Isso pode ser causado por firewall, rede lenta ou servidor indisponível."
  },
  {
    pattern: /fatal: Authentication failed/i,
    title: "Falha na autenticação",
    description: "Suas credenciais Git estão incorretas ou expiraram. Verifique seu usuário/senha ou token de acesso."
  },
  {
    pattern: /remote: Permission.+denied/i,
    title: "Permissão negada",
    description: "Você não tem permissão para acessar este repositório."
  },
  {
    pattern: /fatal: repository.+not found/i,
    title: "Repositório não encontrado",
    description: "O repositório não existe ou você não tem acesso a ele."
  },
  {
    pattern: /Your local changes.+would be overwritten by merge/i,
    title: "Alterações locais conflitantes",
    description: "Você tem modificações locais que conflitam com as alterações do servidor. Faça commit ou stash das suas alterações antes de continuar."
  }
];