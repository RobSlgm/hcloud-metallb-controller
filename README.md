# Hcloud MetalLB Controller

`hcloud-metallb-controller` is a lightweight Kubernetes controller designed to bridge the gap between **MetalLB (L2 Mode)** and **Hetzner Cloud (hcloud)** networking.

It ensures that when MetalLB selects a node to announce an IP address, that IP (either a Floating IP or a Private Network Alias IP) is correctly assigned to the corresponding Hetzner Cloud server via the Hetzner API.

## Why this controller?

Unlike other controllers that implement their own IP selection logic, `hcloud-metallb-controller` is "logic-lite":

- **Trusts MetalLB**: It relies entirely on MetalLB’s existing L2 mechanism to decide which node should host an IP.
- **Single Responsibility**: Its only job is to tell the Hetzner API: "Move this IP to the node MetalLB just picked."
- **Dual Support**: Supports both public Floating IPs and private network Alias IPs.

## Prerequisites

- **Hetzner Cloud Account**: You must have already created a Floating IP or have a Private Network set up.
- **MetalLB**: Installed and configured in L2 mode.
- **Hcloud API Token**: A token with read/write access to your project.

## Installation via Helm

### Helm values

The configuration is kept minimal. You can provide your API key directly or via an existing secret.

1. **Configure the API Key**

Create a `values.yaml` or use the `--set` flag:

```YAML
hcloud:
  # Reference a secret you created manually
  existingSecret: "hcloud"
  # OR provide it directly (not recommended for production)
  # apiKey: "your-hcloud-api-token"
```

> [!Note]
> If using existingSecret, the secret must contain a key named `apiKey`.

2. **Recommended Hardening & Scheduling**

We recommend restricting privileges and spreading the controller across control-plane nodes:

```YAML
# Minimal Privileges
securityContext:
  allowPrivilegeEscalation: false
  capabilities:
    drop: ["ALL"]
  readOnlyRootFilesystem: true
  runAsNonRoot: true
  runAsUser: 1654
  runAsGroup: 1654

# High Availability Scheduling
affinity:
  nodeAffinity:
    preferredDuringSchedulingIgnoredDuringExecution:
      - weight: 100
        preference:
          matchExpressions:
            - key: node-role.kubernetes.io/control-plane
              operator: Exists

topologySpreadConstraints:
  - maxSkew: 1
    topologyKey: "kubernetes.io/hostname"
    whenUnsatisfiable: ScheduleAnyway
    labelSelector:
      matchLabels:
        app.kubernetes.io/instance: hcloud-metallb-controller
```

3. **Replica count**

Adjust to your needs and cluster size (2 is default).

### Installation

See [Helm chart README.md](deploy/README.md).

## Configuration

To activate the controller for a specific MetalLB pool, you must add a specific annotation to your `IPAddressPool` CRD. The controller ignores any pools without these annotations.

### Floating IP (Public)

Use the `closure.ch/pool-type: "FloatingIp"` annotation.

```YAML
apiVersion: metallb.io/v1beta1
kind: IPAddressPool
metadata:
  name: exposed-pool
  namespace: metallb-system
  annotations:
    closure.ch/pool-type: "FloatingIp"
spec:
  addresses:
    - 192.0.2.222-192.0.2.222
```

### Alias IP (Private Network)

Use the `closure.ch/pool-type: "AliasIP"` annotation.

```YAML
apiVersion: metallb.io/v1beta1
kind: IPAddressPool
metadata:
  name: private-pool
  namespace: metallb-system
  annotations:
    closure.ch/pool-type: "AliasIp"
spec:
  addresses:
    - 10.0.0.222-10.0.0.226
```

> [!WARNING]
> **Hetzner Limitation**: A maximum of **5 Alias IPs** can be assigned to a single Hetzner Cloud server at one time. Plan your address pools accordingly.

### Node Matching Logic

For the controller to function, your Kubernetes Node names must correspond to your Hetzner Cloud Server names.

The controller performs a relaxed check to ensure compatibility even if your environment uses FQDNs in one place but not the other. For example, a match is successful if:

- Node name `k8s-node-1` matches Hcloud server `k8s-node-1`.
- Node name `k8s-node-1` matches Hcloud server `k8s-node-1.example.com`.
- Node name `k8s-node-1.internal` matches Hcloud server `k8s-node-1`.

## How it works

1. **MetalLB** assigns an IP from an IPAddressPool to a service and elects a specific node to handle the traffic.

2. **hcloud-metallb-controller** watches for these assignments.

3. The controller identifies the target node and calls the **Hetzner API** to reassign the Floating IP or Alias IP to that specific Virtual Machine.

4. Traffic begins flowing through the Hetzner infrastructure to the correct node.


## Disclaimer

This is an independent open-source project. It is not an official product of [Hetzner Online GmbH](https://hetzner.com) or the [MetalLB project](https://metallb.io).

Use this software at your own risk. The maintainers are not responsible for any service interruptions, unexpected cloud costs, or data loss resulting from the use of this controller. "Hetzner" and "MetalLB" are trademarks of their respective owners.
