# secrets/

Create these two files on the server (never commit them — see `.gitignore`):

```bash
PASS=$(openssl rand -base64 32 | tr -d '/+=')
printf '%s' "$PASS" > db_password.txt
printf 'Host=db;Database=notes;Username=notes;Password=%s' "$PASS" > db_connection.txt
chmod 444 db_password.txt db_connection.txt   # the api runs as UID 1654 and must be able to read them
```
