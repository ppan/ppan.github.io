# Elastic Stack on Ubuntu

## Install ELK

    https://www.elastic.co/guide/en/elasticsearch/reference/7.7/deb.html
    https://www.elastic.co/guide/en/kibana/7.7/deb.html
    https://www.elastic.co/guide/en/logstash/7.7/installing-logstash.html
    sudo systemctl enable elasticsearch.service | kibana.service | logstash.service
    sudo timedatectl set-timezone <my-time-zone>
    sudo vi /etc/systemd/timesyncd.conf (NTP=<my-ntp-server>)
    sudo systemctl restart systemd-timesyncd

## Configure /etc/elasticsearch/elasticsearch.yml

    cluster.name: esdev
    node.name: esdev01
    network.host: <my-ip-address>
    bootstrap.memory_lock: true
    discovery.type: single-node

## Congure /etc/elasticsearch/jvm.options
    -Xms?g
    -Xmx?g

## Security

    sudo ./bin/elasticsearch-certutil ca --pem --days 3650
    openssl pkcs12 -export -out ca.p12 -inkey ca/ca.key -in ca/ca.crt
    sudo ./bin/elasticsearch-certutil cert --ca ~/ca.p12 --pem --multiple --days 3650    (instance:<esdev01>  DNS: esdev01.<my-domain>)
    sudo cp ca/ca.crt esdev01/* /etc/[ELK]/certs
    sudo openssl pkcs8 -in esdev01/esdev01.key -topk8 -nocrypt -out esdev01.pkcs8.key
    sudo chmod go+r /etc/logstash/esdev01.pkcs8.key

    https://www.elastic.co/guide/en/elasticsearch/reference/current/secure-cluster.html
    https://www.elastic.co/guide/en/elasticsearch/reference/current/encrypting-communications-hosts.html
    https://www.elastic.co/blog/configuring-ssl-tls-and-https-to-secure-elasticsearch-kibana-beats-and-logstash
    https://www.howtoforge.com/tutorial/how-to-install-elastic-stack-ubuntu-1804/#install-and-configure-nginx-as-reverseproxy-for-kibana

## NGINX

    https://ssl-config.mozilla.org/
    https://wiki.mozilla.org/Security/Server_Side_TLS
    https://gist.github.com/plentz/6737338#file-nginx-conf
    sudo openssl dhparam -out /etc/nginx/certs/dhparam.pem 2048
    sudo openssl pkcs7 -print_certs -in <my-domain>.cer -outform PEM -out <my-domain>.pem
    https://gist.github.com/junxy/2464633f27345fbe6a98
    winpty openssl pkcs12 -in [yourfile.pfx] -nocerts -out [keyfile-encrypted.key]
    winpty openssl pkcs12 -in [yourfile.pfx] -clcerts -nokeys -out [certificate.crt]
    winpty openssl rsa -in [keyfile-encrypted.key] -out [keyfile-decrypted.key]
    https://www.sslsupportdesk.com/digicert-certificate-utility-ssl-export-instructions-pfx-or-pem-format/




# Elastic Stack on AKS

## Azure setup

    az account set --subscription <subscription-name>
    az aks get-credentials --resource-group <resource-group-name> --name <aks-cluster-name>

AKS Dashboard

    kubectl create clusterrolebinding kubernetes-dashboard --clusterrole=cluster-admin --serviceaccount=kube-system:kubernetes-dashboard
    az aks browse --resource-group <resource-group-name> --name <aks-cluster-name>

## K8S setup

Cluster name: elasticdev

    kubectl apply -f https://download.elastic.co/downloads/eck/1.1.0/all-in-one.yaml
    kubectl -n elastic-system logs -f statefulset.apps/elastic-operator
    
    kubectl apply -f k8s-es.yml
    
    kubectl apply -f k8s-kibana.yml

kubectl get secret elasticdev-es-elastic-user -o go-template='\{\{\.data\.elastic \| base64decode\}\}'

Login Kibana as elastic, create new user logstash_writer, logstash_system.
    
    kubectl apply -f k8s-logstash.yml

[k8s-es.yml](k8s-es.yml)    [k8s-kibana.yml](k8s-kibana.yml)    [k8s-logstash.yml](k8s-logstash.yml)

[Elastic Guide](https://www.elastic.co/guide/en/cloud-on-k8s/current/k8s-deploy-eck.html)

Debugging

    kubectl get event --sort-by .lastTimestamp -w
    kubectl exec -it <pod-name> -- /bin/bash

Delete resources

    kubectl get namespaces --no-headers -o custom-columns=:metadata.name | xargs -n1 kubectl delete elastic --all -n
    kubectl delete elastic --all
