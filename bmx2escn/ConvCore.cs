using System;
using System.Collections.Generic;
using System.Text;

namespace bmx2escn {
    public class ConvCore : IDisposable {
        public ConvCore(CmdArg opts) {
            mBmxReader = new BmxReader(opts.InputBmx);

            if (opts.OutputEscn is null) {
                mEscnWriter = null;
            } else {
                mEscnWriter = new EscnWriter(opts.OutputEscn, opts.EscnInternalPath, mBmxReader.TextureCount, mBmxReader.MaterialCount, mBmxReader.MeshCount);
            }
            if (opts.OutputJson is null) {
                mJsonWriter = null;
            } else {
                mJsonWriter = new GroupJsonWriter(opts.OutputJson, opts.OutputJsonType);
            }
        }

        EscnWriter mEscnWriter;
        GroupJsonWriter mJsonWriter;
        BmxReader mBmxReader;

        public void Dispose() {
            if (!(mEscnWriter is null)) mEscnWriter.Dispose();
            if (!(mJsonWriter is null)) mJsonWriter.Dispose();
        }

        public void DoConv() {
            if (!(mEscnWriter is null)) {
                foreach(var item in mBmxReader.IterateTexture()) {
                    Console.WriteLine($"Processing texture: {item.NAME}(#{item.INDEX})");
                    mEscnWriter.WriteTexture(item);
                }
                foreach (var item in mBmxReader.IterateMaterial()) {
                    Console.WriteLine($"Processing material: {item.NAME}(#{item.INDEX})");
                    mEscnWriter.WriteMaterial(item);
                }
                foreach (var item in mBmxReader.IterateMesh()) {
                    Console.WriteLine($"Processing mesh: {item.NAME}(#{item.INDEX})");
                    mEscnWriter.WriteMesh(item);
                }
            }

            foreach (var item in mBmxReader.IterateObject()) {
                Console.WriteLine($"Processing object: {item.NAME}(#{item.INDEX})");
                if (!(mEscnWriter is null)) {
                    mEscnWriter.WriteObject(item);
                }
                if (!(mJsonWriter is null)) {
                    mJsonWriter.AddObject(item);
                }
            }
        }

    }
}
