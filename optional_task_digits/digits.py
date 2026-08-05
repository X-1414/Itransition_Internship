import argparse
import csv
import glob
import os
import sys
 
import numpy as np
from PIL import Image
from sklearn.datasets import fetch_openml
from sklearn.neural_network import MLPClassifier
from sklearn.model_selection import train_test_split
 
def train_classifier(sample_size=15000):
    print("Downloading/loading MNIST (cached after first run)...")
    mnist = fetch_openml("mnist_784", version=1, as_frame=False, parser="auto")
    X, y = mnist.data, mnist.target.astype(int)
 
    X_sub, _, y_sub, _ = train_test_split(
        X, y, train_size=sample_size, stratify=y, random_state=0
    )
    print(f"Training on {len(X_sub)} MNIST samples...")
    clf = MLPClassifier(hidden_layer_sizes=(128, 64), max_iter=50, random_state=0)
    clf.fit(X_sub, y_sub)
    print(f"Training-set accuracy: {clf.score(X_sub, y_sub):.3f}")
    return clf
 
def load_and_preprocess(path):
    img = Image.open(path).convert("L")
    img28 = img.resize((28, 28), Image.LANCZOS)
    arr = np.array(img28).astype(float)
    return arr.reshape(1, -1)
 
def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("folder", help="Path to folder containing digit images")
    parser.add_argument("--pattern", default="*.jpg", help="Glob pattern (default: *.jpg)")
    parser.add_argument("--out", default="predictions.csv", help="Output CSV path")
    parser.add_argument("--sample-size", type=int, default=15000, help="MNIST training samples to use")
    args = parser.parse_args()
 
    files = sorted(glob.glob(os.path.join(args.folder, args.pattern)))
    if not files:
        print(f"No files matching {args.pattern} found in {args.folder}", file=sys.stderr)
        sys.exit(1)
 
    print(f"Found {len(files)} files.")
    clf = train_classifier(sample_size=args.sample_size)
 
    counts = [0] * 10
    rows = []
    for path in files:
        try:
            features = load_and_preprocess(path)
            pred = int(clf.predict(features)[0])
        except Exception as e:
            print(f"Warning: failed on {path}: {e}", file=sys.stderr)
            continue
        counts[pred] += 1
        rows.append((os.path.basename(path), pred))
 
    with open(args.out, "w", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["filename", "predicted_digit"])
        writer.writerows(rows)
 
    print("\nCounts [0_count, 1_count, ..., 9_count]:")
    print(counts)
    print(f"\nSum of counts: {sum(counts)} (should equal number of files: {len(files)})")
    print(f"Per-file predictions written to {args.out}")
 
if __name__ == "__main__":
    main()
 
