# Etape 1: Utiliser une image Mono pour construire l'application
FROM mono:latest AS build
WORKDIR /src
COPY . .
# Compiler votre application ici (ajustez selon votre solution/project)
RUN msbuild script.sln /p:Configuration=Release

# Etape 2: Créer l'image finale pour l'exécution
FROM mono:latest AS final
WORKDIR /app
COPY --from=build /src/bin/Release .
# Ajustez l'entrée selon le nom de votre exécutable
ENTRYPOINT ["mono", "./script.exe"]
