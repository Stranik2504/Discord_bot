FROM mcr.microsoft.com/dotnet/sdk:5.0
LABEL project="Discord_bot"
LABEL maintainer="Stranik"
WORKDIR Discord_bot_
COPY . .
RUN dotnet restore
CMD dotnet run