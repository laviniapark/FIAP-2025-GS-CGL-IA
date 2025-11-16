# 📘 Documentação do Modelo de IA – Recomendações de Bem-Estar (Gemini AI)

## 1. Visão Geral

O projeto utiliza o modelo Gemini AI, da Google, para gerar recomendações personalizadas de bem-estar emocional.

O modelo não foi treinado manualmente por nós; em vez disso, fazemos uso da API generativa pré-treinada, enviando um prompt cuidadosamente estruturado com informações do usuário.

A função da IA é:
- Analisar o resumo do relato enviado pelo usuário
- Considerar o nível de risco emocional identificado
- Gerar uma mensagem acolhedora, com orientação segura e simples

## 2. Como o modelo foi “treinado” no contexto do projeto

Embora o Gemini seja um modelo de linguagem já treinado com bilhões de dados pela Google, nossa customização é feita via engenharia de prompt.

Ou seja, criamos um prompt que define:
- O papel da IA (assistente de bem-estar emocional, neutra, acolhedora, sem diagnósticos)
- O objetivo da saída (recomendações práticas e seguras)
- O tom da comunicação (calmo, gentil, leve)
- Regras de segurança (não fazer diagnósticos médicos, não incentivar automedicação, etc.)

Prompt utilizado:
```
"Usuário relatou: ""{resumo}""
            Nível emocional: {nivelDescricao}.

            Gere uma recomendação acolhedora e prática:
            - Validação emocional gentil
            - 2 a 4 sugestões simples
            - Para leve: respiração, pequenas pausas, algo prazeroso
            - Para moderado: grounding, pausa estruturada, falar com alguém de confiança
            - Nunca mencione IA ou termos médicos.
            - Escreva 4-6 frases."
```

Essa é a única “configuração” feita por nós.

Não há treinamento supervisionado, ajuste fino ou datasets próprios.

## 3. Fluxo de funcionamento no sistema

### Entrada (JSON)
O modelo recebe:
- `resumoRecebido` (texto do usuário)
- `nivel` (enum de risco: 0 ou 1)

Esses dados são enviados pelo back-end Java para o .NET via requisição REST

### Processamento
1. O .NET monta o prompt com as informações recebidas
2. Envia para a API do Gemini AI
3. Recebe a recomendação gerada
4. Retorna ao Java

### Saída (JSON)
A IA devolve algo como:
```json
{
  "recomendacao": "Sugiro que você faça uma pequena pausa...",
  "sucesso": true
}
```
Depois o Java armazena os dados e envia novamente para o Front

## 4. Como o modelo é utilizado
No código, o uso consiste em:

1. **Criar o cliente da API do Gemini**

No back-end (.NET), é criado um objeto responsável por se comunicar com o serviço do Gemini AI (Client)

2. **Montar o prompt com os dados do usuário**

O resumo escrito pelo usuário e o nível de risco são inseridos dentro de um prompt estruturado, que orienta o modelo sobre como responder (`GeminiAiService.cs`)

3. **Enviar a requisição para o Gemini**

O back-end envia o prompt montado para a API do Gemini. O modelo processa o texto e gera uma recomendação personalizada

4. **Tratar a resposta da IA**

A resposta recebida é convertida em JSON e devolvida ao sistema Java, que envia o conteúdo para o Front e registra os dados em seu banco

O método interno recebe o resumo (string) e o nível de risco (enum convertido para inteiro), e devolve uma string com a recomendação juntamente com um bool de sucesso para registro de histórico local (`IALogs`)

## 5. Limitações
1. A IA não substitui psicólogos ou profissionais de saúde, sendo proibida de oferecer diagnósticos e medicações
2. A resposta depende totalmente da qualidade do resumo enviado
3. Não há controle sobre o treinamento base do modelo
4. Pode haver variações no tom e no nível de detalhe da recomendação
