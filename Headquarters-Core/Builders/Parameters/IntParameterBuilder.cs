using System;
using System.Threading.Tasks;

namespace Headquarters_Core.Builders.Parameters
{
    public class IntParameterBuilder : ParameterBuilder<int>
    {
        public IntParameterBuilder(ParameterBuilderContext builderContext) : base(builderContext) { }

        public override IBuilderContext BuilderContext { get; set; }

        public override async Task<int> BuildAsync(params string[] args)
        {
            if (args.Length != 1)
            {
                return Fail(new ArgumentException($"Cannot convert '[{string.Join(", ", args)}]' to an integer.", nameof(args)));
            }

            if (!int.TryParse(args[0], out int num))
            {
                return Fail(new ArgumentException($"Cannot convert value '{args[0]}' to an integer.", nameof(args)));
            }

            return await Task.FromResult(Succeed(num));
        }
    }
}
