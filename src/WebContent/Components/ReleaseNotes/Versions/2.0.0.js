export default {
  version: "2.0.0",
  date: "23 de Abr. de 2025",
  changes: {
    new: [
      "O layout do sistema foi atualizado para melhorar a experiência do usuário, com novos ícones e cores.",
      "Agora temos uma tela de configurações para facilitar o acesso às configurações que antes eram apenas feitas apenas no config.json",
      "Quer saber as novidades? A tela de notas de release que você está lendo foi criada para você entender o que mudou!",
      "Se der falha, não se preocupe! Agora temos telas de erro que mostram o que aconteceu e como resolver.",
      "Se você usa poucas aplicações, o modo compacto foi criado para você, basta acessar o menu de configurações e ativá-lo.",
      "Notificações vão surgir quando houver alterações nas configurações do sistema, assim você entende o que realmente foi alterado.",
      "Alterou algo nas configurações e não salvou? Não se preocupe, agora temos um aviso que mostra que você não salvou as alterações.",

    ],
    improvement: [
      "Melhorias na performance do sistema, tornando-o mais rápido e responsivo.",
      "A configuração de email agora é feita diretamente na tela de configurações, facilitando o acesso e a atualização.",
      "Adicionado feedback visual para ações de recarga do sistema, melhorando a usabilidade.",
      "As operações de git fetch agora são realizadas de forma mais eficiente e simultânea, reduzindo o tempo de espera.",

    ],
    correction: [
      "Se o sistema iniciar sem um arquivo config.json ou com um arquivo vazio ele vai automaticamente criar um novo arquivo com as configurações padrão.",
      "Se o sistema iniciar com um arquivo config.json inválido, ele vai mostrar uma mensagem de erro devidamente tratada.",
      "Corrigida falta de feedback ao realizar gitpull em sistemas onde ocorriam falhas no git. Agora um modal de erro será exibido no lugar.",
      "Iniciar e pausar aplicações agora tem tratamento dinamico e responde tanto ao fechamento externo do processo quando a ação manual do usuário.",
      "Corrigido falha onde permitia que o usuário abrisse várias instancias do Visual Studio travando o sistema.",
      "Corrigido erro onde o sistema não atualizava corretamente o status de aplicações que falhavam ao iniciar.",
      "Corrigido problema onde várias instancias dotnet eram abertas com múltiplos cliques no botão de iniciar.",
    ]
  }
};