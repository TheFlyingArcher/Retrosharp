# Retrosharp Deployment

## Overview

The goal here is to carry out the Phase 1 Step 8 onto the Raspberry Pi itself. There exists the `docker-compose.yml` completed by Step 8. However, there are other tasks to properly access Retrosharp outside of the Raspberry Pi

## Infrastructure

### Hardware

Retrosharp will sit on an 8 GB Raspberry Pi 4b+ (confirm this). This is a standalone device running Ubuntu Linux as the operating system.

### Network

The Raspberry Pi is a part of a home network configured by a Ubiquity UniFi. I also run my own bind DNS server on another Raspberry Pi. The TLD of my home network is `unionsquared.lan`. The machine FQDN is `usqapp01.unionsquared.lan` in which I have preconfigured on my name server. Retrosharp will have the FQDN `retrosharp.unionsquared.lan`