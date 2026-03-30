FROM mcr.microsoft.com/mssql/server:2019-latest
USER root
WORKDIR /usr/config
COPY . ./

# Grant permissions for to our scripts to be executable
RUN chmod +x entrypoint.sh
RUN chmod +x configure-db.sh
ENV PATH="/gen:$PATH"

ENTRYPOINT ["./entrypoint.sh"]

# Tail the setup logs to trap the process
CMD ["tail -f /dev/null"]

HEALTHCHECK --interval=15s CMD /opt/mssql-tools18/bin/sqlcmd -C -U sa -P $SA_PASSWORD -Q "select 1" && grep -q "MSSQL CONFIG COMPLETE" ./config.log
