# Manually install the following certificate into Trusted People to work with the example code
$cert = New-SelfSignedCertificate -Subject "Temporary WCF Certificate" -DnsName "localhost" -CertStoreLocation "cert:\LocalMachine\My"
$cert | Format-List -Property *

