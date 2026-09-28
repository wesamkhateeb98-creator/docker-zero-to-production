#!/usr/bin/env bash
# Hits https://localhost/ every 100 ms until probe.stop exists; prints a summary.
out=probe.log; : > $out; rm -f probe.stop
while [ ! -f probe.stop ]; do
  r=$(curl -sk -m 2 -w ' %{http_code}' https://localhost/ 2>/dev/null)
  echo "$(date +%s.%N | cut -c1-14) ${r##* } $(echo "$r" | grep -o '"version":"v[0-9]"' | cut -d'"' -f4)" >> $out
  sleep 0.1
done
